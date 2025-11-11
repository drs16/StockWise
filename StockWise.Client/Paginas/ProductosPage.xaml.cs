using StockWise.Client.Services;
using System.Collections.ObjectModel;
using StockWise.Client.Modelo;

namespace StockWise.Client.Paginas;

public partial class ProductosPage : ContentPage
{
    private readonly ApiService _apiService;

    public ObservableCollection<ProductoDto> Productos { get; set; } = new();

    public ProductosPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
        ProductosList.ItemsSource = Productos;
        CargarProductos();
    }

    private async void CargarProductos()
    {
        try
        {
            MessageLabel.Text = "Cargando productos...";
            var productos = await _apiService.GetProductosAsync();

            Productos.Clear();
            foreach (var p in productos)
                Productos.Add(p);

            MessageLabel.Text = "";
        }
        catch (Exception ex)
        {
            MessageLabel.Text = $"Error: {ex.Message}";
        }
    }

    private void OnRefreshClicked(object sender, EventArgs e)
    {
        CargarProductos();
    }
}
