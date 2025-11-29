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

            // Si se envía nueva contraseña
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                if (!Regex.IsMatch(dto.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*_\-]).{8,}$"))
                    return BadRequest("La contraseña no cumple los requisitos de seguridad.");

                using var sha = System.Security.Cryptography.SHA256.Create();
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(dto.Password));
                usuario.PasswordHash = Convert.ToBase64String(bytes);
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
    }
}
