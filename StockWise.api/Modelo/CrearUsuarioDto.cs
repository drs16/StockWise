namespace StockWise.api.Modelo
{
    public class CrearUsuarioDto
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Contraseña en texto plano solo al crear
        public string Password { get; set; } = string.Empty;
    }
}
