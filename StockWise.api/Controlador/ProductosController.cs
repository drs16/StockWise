using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockWise.api.Modelo;
using StockWise.api.Servicios;
using StockWise.Api.Data;
using System.Text;

namespace StockWise.api.Controlador
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly QRService _qrService;

        public ProductosController(AppDbContext context, QRService qrService)
        {
            _context = context;
            _qrService = qrService;
        }

        [HttpGet("{id}/qr")]
        public async Task<IActionResult> ObtenerQR(int id)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
                return NotFound("Producto no encontrado.");

            var qrBytes = _qrService.GenerarQR(producto.CodigoQR);

            return File(qrBytes, "image/png", $"QR_{producto.Nombre}.png");
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
        {
            return await _context.Productos.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Producto>> GetProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound();
            return producto;
        }


        [HttpGet("porQR/{codigo}")]
        public async Task<ActionResult<Producto>> GetPorQR(string codigo)
        {
            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.CodigoQR == codigo);

            if (producto == null)
                return NotFound();

            return producto;
        }

        [HttpGet("exportar")]
        public async Task<IActionResult> ExportarCSV()
        {
            var productos = await _context.Productos.ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Nombre,Proveedor,Cantidad,Precio,CodigoQR");

            foreach (var p in productos)
            {
                sb.AppendLine($"{p.Nombre},{p.Proveedor},{p.Cantidad},{p.Precio},{p.CodigoQR}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());

            return File(bytes, "text/csv", "productos.csv");
        }


        [HttpPost("importar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ImportarProductos([FromBody] List<Producto> productos)
        {
            if (productos == null || !productos.Any())
                return BadRequest("No se recibieron productos para importar.");

            // Validar y generar códigos QR faltantes
            foreach (var producto in productos)
            {
                if (string.IsNullOrWhiteSpace(producto.CodigoQR))
                {
                    producto.CodigoQR = $"{producto.Nombre[..Math.Min(4, producto.Nombre.Length)].ToUpper()}-{Guid.NewGuid().ToString()[..6]}";
                }
            }

            await _context.Productos.AddRangeAsync(productos);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"{productos.Count} productos importados correctamente." });
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<Producto>> PostProducto(ProductoDto dto)
        {
            var producto = new Producto
            {
                Nombre = dto.Nombre,
                Cantidad = dto.Cantidad,
                Precio = dto.Precio,
                Proveedor = dto.Proveedor,
                CodigoQR = string.IsNullOrEmpty(dto.CodigoQR)
                    ? $"{dto.Nombre[..Math.Min(4, dto.Nombre.Length)].ToUpper()}-{Guid.NewGuid().ToString()[..6]}"
                    : dto.CodigoQR,
                EmpresaId = dto.EmpresaId
            };

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, producto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> PutProducto(int id, ProductoDto dto)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
                return NotFound();

            producto.Nombre = dto.Nombre;
            producto.Cantidad = dto.Cantidad;
            producto.Precio = dto.Precio;
            producto.Proveedor = dto.Proveedor;

            if (!string.IsNullOrEmpty(dto.CodigoQR))
                producto.CodigoQR = dto.CodigoQR;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
                return NotFound();

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}