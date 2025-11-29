using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockWise.api.Modelo;
using StockWise.api.Servicios;
using StockWise.Api.Data;
using System.Security.Cryptography;
using System.Text;



namespace StockWise.api.Controlador
{

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;

        public AuthController(AppDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(Usuario usuario)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email))
                return BadRequest("El correo ya está registrado.");

            usuario.PasswordHash = HashPassword(usuario.Password);
            usuario.Password = null; // limpiar texto plano

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok("Usuario registrado correctamente.");
        }


        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == login.Email);

            if (usuario == null || !VerifyPassword(login.Password, usuario.PasswordHash))
                return Unauthorized("Credenciales inválidas.");

            var token = _tokenService.GenerateToken(usuario);
            return Ok(new { token });
        }



        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private bool VerifyPassword(string enteredPassword, string storedHash)
        {
            return HashPassword(enteredPassword) == storedHash;
        }


        [AllowAnonymous]
        [HttpPost("registroInicial")]
        public async Task<ActionResult> RegistroInicial(RegistroInicialDto dto)
        {
            // 1️⃣ Crear empresa
            var empresa = new Empresa
            {
                Nombre = dto.NombreEmpresa,
                NIF = dto.NIF,
                Direccion = dto.Direccion,
                Email = dto.EmailEmpresa,
                Telefono = dto.TelefonoEmpresa
            };

            _context.Empresas.Add(empresa);
            await _context.SaveChangesAsync(); // aquí empresa.Id ya tiene valor

            // 2️⃣ Crear usuario administrador
            var usuarioAdmin = new Usuario
            {
                NombreUsuario = dto.AdminNombre,
                Email = dto.AdminEmail,
                PasswordHash = HashPassword(dto.Password),
                Rol = "Administrador",
                EmpresaId = empresa.Id
            };

            _context.Usuarios.Add(usuarioAdmin);
            await _context.SaveChangesAsync();

            // 3️⃣ Crear token JWT
            var token = _tokenService.GenerateToken(usuarioAdmin);

            // 4️⃣ Devolverlo todo
            return Ok(new
            {
                empresaId = empresa.Id,
                usuarioId = usuarioAdmin.Id,
                token = token
            });
        }

    }

}
