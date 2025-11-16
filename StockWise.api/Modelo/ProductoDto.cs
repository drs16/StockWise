namespace StockWise.api.Modelo
{
    public class ProductoDto
    {
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public string Proveedor { get; set; } = string.Empty;
        public string CodigoQR { get; set; } = string.Empty;
        public int EmpresaId { get; set; }
    }
}
