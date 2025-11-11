namespace StockWise.Client.Modelo;

public class ProductoDto
{
    public int id { get; set; }
    public string nombre { get; set; }
    public int cantidad { get; set; }
    public decimal precio { get; set; }
    public string proveedor { get; set; }
}
