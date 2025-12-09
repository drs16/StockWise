using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockWise.api.Modelo;
using StockWise.Api.Data;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace StockWise.api.Controlador
{
    // 👇 Ahora cualquiera autenticado puede entrar al controlador,
    // pero SOLO los métodos marcados requieren rol Administrador
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        // 📌 Obtener EmpresaId desde el token
        private int ObtenerEmpresaId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "EmpresaId")?.Value;
            if (string.IsNullOrEmpty(claim))
                throw new Exception("No se pudo obtener EmpresaId del token.");
            return int.Parse(claim);
        }

        // 📌 Obtener UsuarioId desde el token
        private int ObtenerUsuarioId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claim))
                throw new Exception("No se pudo obtener UsuarioId del token.");
            return int.Parse(claim);
        }

        // ======================================================
        // 📌 MÉTODOS SOLO PARA ADMINISTRADOR
        // ======================================================

        // Obtener todos los usuarios de la empresa
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            int empresaId = ObtenerEmpresaId();

            return await _context.Usuarios
                .Where(u => u.EmpresaId == empresaId)
                .ToListAsync();
        }

        // Obtener un usuario por Id
        [Authorize(Roles = "Administrador")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            int empresaId = ObtenerEmpresaId();

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id && u.EmpresaId == empresaId);

            if (usuario == null)
                return NotFound();

            return usuario;
        }

        // Crear un usuario
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> PostUsuario(CrearUsuarioDto dto)
        {
            // Validar email
            if (!Regex.IsMatch(dto.Email ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return BadRequest("El email no es válido.");

            // Email duplicado
            if (await _context.Usuarios.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("El email ya está registrado.");

            int empresaId = ObtenerEmpresaId();

            // Generar contraseña temporal automática
            string tempPass = GenerarPasswordTemporal();

            using var sha = SHA256.Create();
            string hash = Convert.ToBase64String(
                sha.ComputeHash(Encoding.UTF8.GetBytes(tempPass))
            );

            var usuario = new Usuario
            {
                NombreUsuario = dto.NombreUsuario,
                Email = dto.Email,
                EmpresaId = empresaId,
                Rol = "Empleado",
                PasswordHash = hash,
                DebeCambiarPassword = true
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // 👉 devolver la contraseña temporal
            return Ok(new { tempPassword = tempPass });
        }


        // Editar usuario
        [Authorize(Roles = "Administrador")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, EditarUsuarioDto dto)
        {
            int empresaId = ObtenerEmpresaId();

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id && u.EmpresaId == empresaId);

            if (usuario == null)
                return NotFound();

            usuario.NombreUsuario = dto.NombreUsuario;

            if (usuario.Email != dto.Email)
            {
                if (!Regex.IsMatch(dto.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    return BadRequest("Email no válido.");

                if (await _context.Usuarios.AnyAsync(u => u.Email == dto.Email && u.Id != id))
                    return BadRequest("Email ya registrado.");

                usuario.Email = dto.Email;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Eliminar usuario
        [Authorize(Roles = "Administrador")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            int empresaId = ObtenerEmpresaId();

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id && u.EmpresaId == empresaId);

            if (usuario == null)
                return NotFound();

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Reset password (admin)
        [Authorize(Roles = "Administrador")]
        [HttpPost("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return NotFound("Usuario no encontrado.");

            string tempPass = GenerarPasswordTemporal();

            using var sha = SHA256.Create();
            usuario.PasswordHash = Convert.ToBase64String(
                sha.ComputeHash(Encoding.UTF8.GetBytes(tempPass))
            );

            usuario.DebeCambiarPassword = true;

            await _context.SaveChangesAsync();

            return Ok(new { tempPassword = tempPass });
        }



        private string GenerarPasswordTemporal()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var pass = new char[4]; 

            for (int i = 0; i < pass.Length; i++)
                pass[i] = chars[random.Next(chars.Length)];

            return "Tmp" + new string(pass);
        }


        // ======================================================
        // 📌 MÉTODOS PARA CUALQUIER USUARIO AUTENTICADO
        // ======================================================

        // Cambiar su propia contraseña
        [Authorize] // cualquier usuario logueado
        [HttpPost("cambiarPassword")]
        public async Task<IActionResult> CambiarPassword(CambiarPasswordDto dto)
        {
            int empresaId = ObtenerEmpresaId();
            int usuarioId = ObtenerUsuarioId();

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == usuarioId && u.EmpresaId == empresaId);

            if (usuario == null)
                return Unauthorized();

            // Hash de la nueva contraseña
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(dto.NuevaPassword));
            usuario.PasswordHash = Convert.ToBase64String(bytes);

            usuario.DebeCambiarPassword = false;

            await _context.SaveChangesAsync();

            return NoContent();
        }



    }
}
