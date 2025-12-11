using StockWise.Client.Services;

namespace StockWise.Client.Paginas;

public partial class MiPerfilPage : ContentPage
{
    private readonly ApiService _apiService;

    public MiPerfilPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        NombreLabel.Text = "Usuario: " + await SecureStorage.GetAsync("usuario_nombre");
        EmailLabel.Text = "Email: " + await SecureStorage.GetAsync("usuario_email");
        RolLabel.Text = "Rol: " + await SecureStorage.GetAsync("usuario_rol");

        var empresa = await _apiService.ObtenerMiEmpresaAsync();

        EmpresaLabel.Text = empresa != null
            ? "Empresa: " + empresa.Nombre
            : "Empresa: (No encontrada)";
    }


}