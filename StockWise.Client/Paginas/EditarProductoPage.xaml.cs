using StockWise.Client.Modelo;
using StockWise.Client.Services;

namespace StockWise.Client.Paginas;

public partial class EditarProductoPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly int _productoId;

    public EditarProductoPage(ApiService apiService, ProductoDto producto)
    {
        InitializeComponent();
        _apiService = apiService;
        _productoId = producto.Id;

        // Rellenar los campos con info del producto
        NombreEntry.Text = producto.Nombre;
        ProveedorEntry.Text = producto.Proveedor;
        CantidadEntry.Text = producto.Cantidad.ToString();
        PrecioEntry.Text = producto.Precio.ToString();
    }

    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        var productoEditado = new ProductoDto
        {
            Id = _productoId,
            Nombre = NombreEntry.Text,
            Proveedor = ProveedorEntry.Text,
            Cantidad = int.Parse(CantidadEntry.Text),
            Precio = decimal.Parse(PrecioEntry.Text),
            CodigoQR = ""
        };

        var exito = await _apiService.EditarProductoAsync(_productoId, productoEditado);

        if (exito)
        {
            await DisplayAlert("Éxito", "Producto modificado correctamente.", "OK");
            await Shell.Current.GoToAsync("//productos");
        }
        else
        {
            await DisplayAlert("Error", "No se pudo modificar el producto.", "OK");
        }
    }
}