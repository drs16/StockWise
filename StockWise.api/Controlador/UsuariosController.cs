using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockWise.api.Modelo;
using StockWise.Api.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace StockWise.api.Controlador
{
    [Authorize(Roles = "Administrador")]
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

        // GET: api/Usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            int empresaId = ObtenerEmpresaId();

            return await _context.Usuarios
                .Where(u => u.EmpresaId == empresaId)
                .ToListAsync();
        }

        // GET: api/Usuarios/5
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

        // POST: api/Usuarios
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(CrearUsuarioDto dto)
        {
            // Validar email
            if (!Regex.IsMatch(dto.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return BadRequest("El email no es válido.");

            // Email duplicado
            if (await _context.Usuarios.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("El email ya está registrado.");

            // Validar contraseña segura
            if (!Regex.IsMatch(dto.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*_\-]).{8,}$"))
                return BadRequest("La contraseña debe tener mínimo 8 caracteres, una mayúscula, una minúscula, un número y un símbolo.");

            int empresaId = ObtenerEmpresaId();

            var usuario = new Usuario
            {
                NombreUsuario = dto.NombreUsuario,
                Email = dto.Email,
                EmpresaId = empresaId,
                Rol = "Empleado"
            };

            // Hash contraseña
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(dto.Password));
            usuario.PasswordHash = Convert.ToBase64String(bytes);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, usuario);
        }

        // PUT: api/Usuarios/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, EditarUsuarioDto dto)
        {
            int empresaId = ObtenerEmpresaId();

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id && u.EmpresaId == empresaId);

            if (usuario == null)
                return NotFound();

            // Actualizar nombre
            usuario.NombreUsuario = dto.NombreUsuario;

            // Validar email si cambia
            if (usuario.Email != dto.Email)
            {
                if (!Regex.IsMatch(dto.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    return BadRequest("Email no válido.");

                if (await _context.Usuarios.AnyAsync(u => u.Email == dto.Email && u.Id != id))
                    return BadRequest("El email ya está en uso.");

                usuario.Email = dto.Email;
            }



            usuario.Rol = "Empleado"; // Nunca se cambia por API

            await _context.SaveChangesAsync();

            return NoContent();
        }


        // DELETE: api/Usuarios/5
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

        [HttpPost("{id}/reset-password")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return NotFound();

            // Generar contraseña temporal
            string tempPass = Guid.NewGuid().ToString().Substring(0, 8);

            using var sha = System.Security.Cryptography.SHA256.Create();
            usuario.PasswordHash = Convert.ToBase64String(
                sha.ComputeHash(Encoding.UTF8.GetBytes(tempPass))
            );

            usuario.DebeCambiarPassword = true; // si quieres obligarlo a cambiarla luego

            await _context.SaveChangesAsync();

            return Ok(new { tempPassword = tempPass });
        }

        [HttpPost("resetPassword/{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ResetearPassword(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return NotFound("Usuario no encontrado.");

            // Nueva contraseña temporal
            string tempPassword = "Tmp#" + new Random().Next(1000, 9999);

            // Hash
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(tempPassword));
            usuario.PasswordHash = Convert.ToBase64String(bytes);

            // Marcar para cambio obligatorio
            usuario.DebeCambiarPassword = true;

            await _context.SaveChangesAsync();

            return Ok(new { temporal = tempPassword });
        }

        [HttpPost("cambiarPassword")]
        public async Task<IActionResult> CambiarPassword(CambiarPasswordDto dto)
        {
            int empresaId = ObtenerEmpresaId();
            int usuarioId = ObtenerUsuarioId();

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == usuarioId && u.EmpresaId == empresaId);

            if (usuario == null)
                return Unauthorized();

            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(dto.NuevaPassword));
            usuario.PasswordHash = Convert.ToBase64String(bytes);

            usuario.DebeCambiarPassword = false;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private int ObtenerUsuarioId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;

            if (string.IsNullOrEmpty(claim))
                throw new Exception("No se pudo obtener UsuarioId del token.");

            return int.Parse(claim);
        }


    }
}
