using StockWise.Client.Modelo;
using StockWise.Client.Services;

namespace StockWise.Client.Paginas;

public partial class ProductoDetallePage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly ProductoDto _producto;

    public ProductoDetallePage(ProductoDto producto, ApiService apiService)
    {
        InitializeComponent();
        _producto = producto;
        _apiService = apiService;

        CargarDatos();
    }

    private void CargarDatos()
    {
        NombreLabel.Text = _producto.Nombre;
        ProveedorLabel.Text = _producto.Proveedor;
        CantidadLabel.Text = _producto.Cantidad.ToString();
        PrecioLabel.Text = _producto.Precio.ToString("0.00 €");

        // Mostrar QR como imagen (debes generarlo en tu API o localmente)
        if (!string.IsNullOrEmpty(_producto.CodigoQR))
        {
            QrImage.Source = $"https://api.qrserver.com/v1/create-qr-code/?size=200x200&data={_producto.CodigoQR}";
        }
    }

    private async void OnEditarClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(
            new EditarProductoPage(_apiService, _producto)
        );
    }
    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }




    private async void OnEliminarClicked(object sender, EventArgs e)
    {
        var confirmar = await DisplayAlert("Eliminar producto",
            "¿Seguro que deseas eliminar este producto?", "Sí", "No");

        if (!confirmar)
            return;

        var exito = await _apiService.EliminarProductoAsync(_producto.Id);

        if (exito)
        {
            await DisplayAlert("Éxito", "Producto eliminado.", "OK");
            await Shell.Current.GoToAsync(".."); // volver atrás
        }
        else
        {
            await DisplayAlert("Error", "No se pudo eliminar el producto.", "OK");
        }
    }
}
