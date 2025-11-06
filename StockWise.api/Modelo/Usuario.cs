namespace StockWise.api.Modelo
{
    public class Usuario
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Rol { get; set; } = "Empleado"; // "Administrador" o "Empleado"

        public int EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }
    }
}
