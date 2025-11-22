using StockWise.api.Modelo;
using System.ComponentModel.DataAnnotations.Schema;

public class Usuario
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // NO se envía desde el cliente
    public string PasswordHash { get; set; } = string.Empty;

    // Sí se envía desde el cliente, pero NO se guarda
    [NotMapped]
    public string Password { get; set; } = string.Empty;

    public string Rol { get; set; } = "Empleado";
    public int EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }
}
