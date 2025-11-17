using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockWise.api.Modelo;
using StockWise.Api.Data;
using System.Text;


namespace StockWise.Api.Controlador
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmpresasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmpresasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Empresa>>> GetEmpresas()
        {
            return await _context.Empresas.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Empresa>> GetEmpresa(int id)
        {
            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa == null)
                return NotFound();
            return empresa;
        }

        [HttpPost]
        public async Task<ActionResult<Empresa>> PostEmpresa(Empresa empresa)
        {
            _context.Empresas.Add(empresa);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEmpresa), new { id = empresa.Id }, empresa);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> PutEmpresa(int id, Empresa dto)
        {
            var empresa = await _context.Empresas.FindAsync(id);

            if (empresa == null)
                return NotFound("Empresa no encontrada.");

            // Actualizar campos
            empresa.Nombre = dto.Nombre;
            empresa.NIF = dto.NIF;
            empresa.Direccion = dto.Direccion;
            empresa.Email = dto.Email;
            empresa.Telefono = dto.Telefono;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Empresa actualizada con éxito" });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmpresa(int id)
        {
            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa == null)
                return NotFound();

            _context.Empresas.Remove(empresa);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("exportar/{empresaId}")]
        public async Task<IActionResult> ExportarCSVPorEmpresa(int empresaId)
        {
            var productos = await _context.Productos
                .Where(p => p.EmpresaId == empresaId)
                .ToListAsync();

            if (!productos.Any())
                return NotFound("No hay productos para exportar.");

            var sb = new StringBuilder();
            sb.AppendLine("Nombre,Proveedor,Cantidad,Precio,CodigoQR");

            foreach (var p in productos)
            {
                sb.AppendLine($"{p.Nombre},{p.Proveedor},{p.Cantidad},{p.Precio},{p.CodigoQR}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());

            return File(bytes, "text/csv", $"productos_empresa_{empresaId}.csv");
        }

        [HttpGet("miEmpresa")]
        public async Task<ActionResult<Empresa>> ObtenerMiEmpresa()
        {
            var empresaIdClaim = User.Claims.FirstOrDefault(c => c.Type == "EmpresaId")?.Value;

            if (empresaIdClaim == null)
                return Unauthorized("No se pudo obtener EmpresaId del token.");

            int empresaId = int.Parse(empresaIdClaim);

            var empresa = await _context.Empresas.FindAsync(empresaId);

            if (empresa == null)
                return NotFound("Empresa no encontrada.");

            return Ok(empresa);
        }


    }


}