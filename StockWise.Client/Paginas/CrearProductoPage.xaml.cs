using StockWise.Client.Modelo;
using StockWise.Client.Services;

namespace StockWise.Client.Paginas;

public partial class CrearProductoPage : ContentPage
{
    private readonly ApiService _apiService;

    public CrearProductoPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        var producto = new ProductoDto
        {
            Nombre = NombreEntry.Text,
            Proveedor = ProveedorEntry.Text,
            Cantidad = int.Parse(CantidadEntry.Text),
            Precio = decimal.Parse(PrecioEntry.Text),
            EmpresaId = 1
        };

        if (await _apiService.CrearProductoAsync(producto))
        {
            await DisplayAlert("Éxito", "Producto creado", "OK");
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Error", "No se pudo crear el producto", "OK");
        }
    }
}
