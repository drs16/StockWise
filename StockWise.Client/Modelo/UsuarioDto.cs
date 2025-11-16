namespace StockWise.Client.Modelo
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;

        // No se usa en tabla, pero sirve para envio
        public string PasswordHash { get; set; } = string.Empty;

        public int EmpresaId { get; set; }
    }
}
