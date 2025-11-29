namespace StockWise.api.Modelo
{
    public class EditarUsuarioDto
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Solo si el usuario desea cambiarla
        public string? Password { get; set; }
    }
}
