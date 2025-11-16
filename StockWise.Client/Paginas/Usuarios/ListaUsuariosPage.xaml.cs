using StockWise.Client.Modelo;
using StockWise.Client.Services;

namespace StockWise.Client.Paginas.Usuarios;

public partial class ListaUsuariosPage : ContentPage
{
    private readonly ApiService _apiService;
    private int _empresaId;
    private bool menuVisible = false;

    public List<UsuarioDto> Usuarios { get; set; } = new();

    public ListaUsuariosPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarUsuarios();
    }

    private async Task CargarUsuarios()
    {
        Loading();

       var empresaIdString = await SecureStorage.GetAsync("empresa_id");

        if (string.IsNullOrEmpty(empresaIdString))
        {
            await DisplayAlert("Error", "No se encontró el ID de la empresa. Debes iniciar sesión otra vez.", "OK");
            await Shell.Current.GoToAsync("//login");
            return;
        }

        _empresaId = int.Parse(empresaIdString);

        var lista = await _apiService.GetUsuariosAsync();

        Usuarios = lista
            .Where(u => u.EmpresaId == _empresaId && u.Rol != "Administrador")
            .ToList();

        UsuariosList.ItemsSource = Usuarios;

        Loaded();
    }

    private void Loading()
    {
        // Optional spinner
    }

    private void Loaded()
    {
        // hide spinner
    }

    private async void OnEditarClicked(object sender, EventArgs e)
    {
        var usuario = ((Button)sender).BindingContext as UsuarioDto;

        await Navigation.PushAsync(new EditarUsuarioPage(_apiService, usuario));
    }


    private async void OnCrearUsuarioClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CrearUsuarioPage(_apiService));
    }



    private async void OnEliminarClicked(object sender, EventArgs e)
    {
        var usuario = ((Button)sender).BindingContext as UsuarioDto;

        bool confirmar = await DisplayAlert("Eliminar",
            $"¿Eliminar al usuario {usuario.NombreUsuario}?",
            "Sí", "No");

        if (!confirmar) return;

        if (await _apiService.EliminarUsuarioAsync(usuario.Id))
            await CargarUsuarios();
    }
    private void OnMenuToggleClicked(object sender, EventArgs e)
    {
        menuVisible = !menuVisible;
        MenuContainer.IsVisible = menuVisible;
    }

    private async void OnPerfilClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Mi perfil", "Aquí mostraremos los datos del usuario actual 👤", "OK");
    }

    private async void OnAdminClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ListaUsuariosPage(_apiService)); // Si estás ya aquí, no hagas nada
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        SecureStorage.RemoveAll();
        await Shell.Current.GoToAsync("//login");
    }

    private async void OnProductosClicked(object sender, EventArgs e)
    {


        menuVisible = false;
        MenuContainer.IsVisible = false;

        await Shell.Current.GoToAsync("//productos");
    }

}
