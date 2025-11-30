namespace StockWise.Client.Modelo
{
    public class RegistroInicialDto
    {
        // Empresa
        public string NombreEmpresa { get; set; } = string.Empty;
        public string NIF { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string EmailEmpresa { get; set; } = string.Empty;
        public string TelefonoEmpresa { get; set; } = string.Empty;

        // Administrador
        public string AdminNombre { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }


    public class RegistroInicialRespuesta
    {
        public int EmpresaId { get; set; }
        public int UsuarioId { get; set; }
        public string Token { get; set; } = string.Empty;
    }


}