namespace StockWise.api.Modelo
{
    public class Empresa
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string NIF { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;

        public ICollection<Usuario>? Usuarios { get; set; }
        public ICollection<Producto>? Productos { get; set; }
    }
}
