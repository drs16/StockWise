using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using StockWise.Api.Data;
using StockWise.api.Modelo;

namespace StockWise.api.Controlador
{
    [Route("import")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly AppDbContext _pg;

        public ImportController(AppDbContext pg)
        {
            _pg = pg;
        }

        [HttpPost("sqlite")]
        public IActionResult ImportSqlite()
        {
            var sqlitePath = Path.Combine("Data", "stockwise.db");

            if (!System.IO.File.Exists(sqlitePath))
                return NotFound("El archivo SQLite no existe en /Data/stockwise.db");

            using var sqlite = new SqliteConnection($"Data Source={sqlitePath}");
            sqlite.Open();

            // =======================================================
            //                IMPORTAR EMPRESAS
            // =======================================================
            var cmdEmp = sqlite.CreateCommand();
            cmdEmp.CommandText =
                "SELECT Id, Nombre, NIF, Direccion, Email, Telefono FROM Empresas";

            using (var reader = cmdEmp.ExecuteReader())
            {
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);

                    if (!_pg.Empresas.Any(e => e.Id == id))
                    {
                        _pg.Empresas.Add(new Empresa
                        {
                            Id = id,
                            Nombre = reader.GetString(1),
                            NIF = reader.GetString(2),
                            Direccion = reader.GetString(3),
                            Email = reader.GetString(4),
                            Telefono = reader.GetString(5)
                        });
                    }
                }
            }

            _pg.SaveChanges();

            // =======================================================
            //                IMPORTAR USUARIOS
            // =======================================================
            var cmdUsr = sqlite.CreateCommand();
            cmdUsr.CommandText =
                "SELECT Id, NombreUsuario, Email, PasswordHash, Rol, EmpresaId FROM Usuarios";

            using (var reader = cmdUsr.ExecuteReader())
            {
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);

                    if (!_pg.Usuarios.Any(u => u.Id == id))
                    {
                        _pg.Usuarios.Add(new Usuario
                        {
                            Id = id,
                            NombreUsuario = reader.GetString(1),
                            Email = reader.GetString(2),
                            PasswordHash = reader.GetString(3),
                            Rol = reader.GetString(4),
                            EmpresaId = reader.GetInt32(5)
                        });
                    }
                }
            }

            _pg.SaveChanges();

            // =======================================================
            //                IMPORTAR PRODUCTOS
            // =======================================================
            var cmdProd = sqlite.CreateCommand();
            cmdProd.CommandText =
                "SELECT Id, Nombre, Cantidad, Precio, Proveedor, CodigoQR, EmpresaId FROM Productos";

            using (var reader = cmdProd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);

                    if (!_pg.Productos.Any(p => p.Id == id))
                    {
                        _pg.Productos.Add(new Producto
                        {
                            Id = id,
                            Nombre = reader.GetString(1),
                            Cantidad = reader.GetInt32(2),
                            Precio = reader.GetDecimal(3),
                            Proveedor = reader.GetString(4),
                            CodigoQR = reader.GetString(5),
                            EmpresaId = reader.GetInt32(6)
                        });
                    }
                }
            }

            _pg.SaveChanges();

            return Ok("✔ Importación desde SQLite completada correctamente.");
        }
    }
}
