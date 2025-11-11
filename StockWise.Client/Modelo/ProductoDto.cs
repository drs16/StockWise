namespace StockWise.Client.Modelo;

public class ProductoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Precio { get; set; }
    public string Proveedor { get; set; } = string.Empty;
}
