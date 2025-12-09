using StockWise.Client.Modelo;
using StockWise.Client.Services;

namespace StockWise.Client.Paginas;

public partial class MovimientoStockPage : ContentPage
{
    private readonly ApiService _api;
    private List<MovimientoStockDto> _todos;

    public MovimientoStockPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarMovimientos();
    }

    private async Task CargarMovimientos()
    {
        var empresaId = int.Parse(await SecureStorage.GetAsync("empresa_id"));
        _todos = await _api.GetMovimientosAsync(empresaId);

        MovimientosList.ItemsSource = _todos;
    }

    private void OnBuscarChanged(object sender, TextChangedEventArgs e)
    {
        if (_todos == null) return;

        var texto = e.NewTextValue?.ToLower() ?? "";

        MovimientosList.ItemsSource = string.IsNullOrWhiteSpace(texto)
            ? _todos
            : _todos.Where(m => m.NombreUsuario.ToLower().Contains(texto));
    }
}
