namespace StockWise.api.Modelo
{
    public class MovimientoStock
    {
        public int Id { get; set; }

        public int EmpresaId { get; set; }
        public int UsuarioId { get; set; }
        public int ProductoId { get; set; }

        public int CantidadAntes { get; set; }
        public int CantidadDespues { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public string NombreUsuario { get; set; }
        public string NombreProducto { get; set; }
    }

}
