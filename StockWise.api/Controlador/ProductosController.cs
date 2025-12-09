using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockWise.api.Modelo;
using StockWise.api.Servicios;
using StockWise.Api.Data;
using System.Security.Claims;
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

        private int ObtenerEmpresaId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "EmpresaId")?.Value;
            if (string.IsNullOrEmpty(claim))
                throw new Exception("No se pudo obtener EmpresaId del token.");
            return int.Parse(claim);
        }

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
            int empresaId = ObtenerEmpresaId();

            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.CodigoQR == codigo && p.EmpresaId == empresaId);

            if (producto == null)
                return NotFound("Producto no encontrado o no pertenece a tu empresa.");

            return producto;
        }

        private int ObtenerUsuarioId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claim))
                throw new Exception("No se pudo obtener UsuarioId del token.");
            return int.Parse(claim);
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

            int empresaId = productos.First().EmpresaId;

            // Obtener productos existentes de esa empresa
            var existentes = await _context.Productos
                .Where(p => p.EmpresaId == empresaId)
                .ToListAsync();

            var nuevos = new List<Producto>();

            foreach (var p in productos)
            {
                // GENERAR QR si falta
                if (string.IsNullOrWhiteSpace(p.CodigoQR))
                    p.CodigoQR = $"{p.Nombre[..Math.Min(4, p.Nombre.Length)].ToUpper()}-{Guid.NewGuid().ToString()[..6]}";

                // COMPROBAR DUPLICADO POR QR
                if (existentes.Any(e => e.CodigoQR == p.CodigoQR))
                    continue; // ❌ ignorar este producto

                // OPCIONAL: comprobar por nombre + proveedor
                if (existentes.Any(e => e.Nombre == p.Nombre && e.Proveedor == p.Proveedor))
                    continue;

                nuevos.Add(p);
            }

            if (!nuevos.Any())
                return BadRequest("Todos los productos estaban duplicados.");

            await _context.Productos.AddRangeAsync(nuevos);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"{nuevos.Count} productos importados. {productos.Count - nuevos.Count} duplicados ignorados."
            });
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
            int empresaId = ObtenerEmpresaId();
            int usuarioId = ObtenerUsuarioId();

            var usuarioNombre = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "Desconocido";

            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == id && p.EmpresaId == empresaId);

            if (producto == null)
                return NotFound("Producto no encontrado o no pertenece a tu empresa.");

            // Guardar cantidad anterior
            int cantidadAntes = producto.Cantidad;

            // ========== ACTUALIZAR PRODUCTO ==========
            producto.Nombre = dto.Nombre;
            producto.Cantidad = dto.Cantidad;
            producto.Precio = dto.Precio;
            producto.Proveedor = dto.Proveedor;

            if (!string.IsNullOrEmpty(dto.CodigoQR))
                producto.CodigoQR = dto.CodigoQR;

            // ========== REGISTRAR MOVIMIENTO ==========
            var mov = new MovimientoStock
            {
                EmpresaId = empresaId,
                UsuarioId = usuarioId,
                ProductoId = producto.Id,
                CantidadAntes = cantidadAntes,
                CantidadDespues = dto.Cantidad,
                Fecha = DateTime.UtcNow,
                NombreUsuario = usuarioNombre,
                NombreProducto = producto.Nombre
            };

            _context.MovimientosStock.Add(mov);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Producto actualizado", movimientoId = mov.Id });
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

        [HttpGet("porEmpresa/{empresaId}")]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductosPorEmpresa(int empresaId)
        {
            var productos = await _context.Productos
                .Where(p => p.EmpresaId == empresaId)
                .ToListAsync();

            return Ok(productos);
        }

        [HttpGet("exportar/{empresaId}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ExportarCSV(int empresaId)
        {
            var productos = await _context.Productos
                .Where(p => p.EmpresaId == empresaId)
                .ToListAsync();

            if (!productos.Any())
                return BadRequest("No hay productos para exportar.");

            var sb = new StringBuilder();
            sb.AppendLine("Nombre,Proveedor,Cantidad,Precio,CodigoQR");

            foreach (var p in productos)
                sb.AppendLine($"{p.Nombre},{p.Proveedor},{p.Cantidad},{p.Precio},{p.CodigoQR}");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());

            return File(bytes, "text/csv", $"productos_empresa_{empresaId}.csv");
        }

        // ===============================
        // 📌 Obtener movimientos por empresa (EN PRODUCTOS CONTROLLER)
        // ===============================
        [HttpGet("movimientos/{empresaId}")]
        public async Task<ActionResult<IEnumerable<MovimientoStock>>> GetMovimientos(int empresaId)
        {
            var movimientos = await _context.MovimientosStock
                .Where(m => m.EmpresaId == empresaId)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();

            return Ok(movimientos);
        }


    }
}