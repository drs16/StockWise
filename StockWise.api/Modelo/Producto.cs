namespace StockWise.api.Modelo
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public string Proveedor { get; set; } = string.Empty;
        public string CodigoQR { get; set; } = string.Empty;

        public int EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }
    }
}
