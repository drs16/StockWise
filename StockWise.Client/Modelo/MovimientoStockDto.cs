namespace StockWise.Client.Modelo
{
    public class MovimientoStockDto
    {
        public string NombreProducto { get; set; }
        public string NombreUsuario { get; set; }
        public int CantidadAntes { get; set; }
        public int CantidadDespues { get; set; }
        public string Diferencia => $"{CantidadDespues - CantidadAntes:+#;-#;0}";
        public DateTime Fecha { get; set; }
    }

}
