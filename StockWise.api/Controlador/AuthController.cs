using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockWise.api.Modelo;
using StockWise.api.Servicios;
using StockWise.Api.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;



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
            // ===========================================
            // 1️⃣ VALIDACIONES DE EMPRESA
            // ===========================================

            // NIF válido
            if (!Regex.IsMatch(dto.NIF ?? "", @"^[0-9]{8}[A-Za-z]$"))
                return BadRequest("El NIF no es válido. Debe tener 8 números y una letra.");

            // NIF duplicado
            if (await _context.Empresas.AnyAsync(e => e.NIF == dto.NIF))
                return BadRequest("Ya existe una empresa registrada con ese NIF.");

            // Nombre empresa duplicado
            if (await _context.Empresas.AnyAsync(e => e.Nombre == dto.NombreEmpresa))
                return BadRequest("El nombre de la empresa ya está registrado.");

            // Email empresa válido
            if (!Regex.IsMatch(dto.EmailEmpresa ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return BadRequest("El email de la empresa no es válido.");

            // ===========================================
            // 2️⃣ VALIDACIONES DE USUARIO ADMIN
            // ===========================================

            // Email admin válido
            if (!Regex.IsMatch(dto.AdminEmail ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return BadRequest("El email del administrador no es válido.");

            // Email admin duplicado
            if (await _context.Usuarios.AnyAsync(u => u.Email == dto.AdminEmail))
                return BadRequest("El email del administrador ya está registrado.");

            // Contraseña segura
            if (!Regex.IsMatch(dto.Password ?? "",
                @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*_\-]).{8,}$"))
            {
                return BadRequest("La contraseña debe tener al menos 8 caracteres, " +
                                  "incluyendo mayúscula, minúscula, número y un símbolo.");
            }

            // ===========================================
            // 3️⃣ CREAR EMPRESA
            // ===========================================

            var empresa = new Empresa
            {
                Nombre = dto.NombreEmpresa,
                NIF = dto.NIF,
                Direccion = dto.Direccion,
                Email = dto.EmailEmpresa,
                Telefono = dto.TelefonoEmpresa
            };

            _context.Empresas.Add(empresa);
            await _context.SaveChangesAsync(); // ahora empresa.Id ya existe

            // ===========================================
            // 4️⃣ CREAR USUARIO ADMINISTRADOR
            // ===========================================

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

            // ===========================================
            // 5️⃣ GENERAR TOKEN
            // ===========================================

            var token = _tokenService.GenerateToken(usuarioAdmin);

            return Ok(new
            {
                empresaId = empresa.Id,
                usuarioId = usuarioAdmin.Id,
                token = token
            });
        }


    }

}
