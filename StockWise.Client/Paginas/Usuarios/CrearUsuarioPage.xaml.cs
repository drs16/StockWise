using StockWise.Client.Modelo;
using StockWise.Client.Services;

namespace StockWise.Client.Paginas.Usuarios;

public partial class CrearUsuarioPage : ContentPage
{
    private readonly ApiService _apiService;

    public CrearUsuarioPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    private async void OnCrearClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NombreEntry.Text) ||
            string.IsNullOrWhiteSpace(EmailEntry.Text))
        {
            await DisplayAlert("Error", "Por favor completa todos los campos.", "OK");
            return;
        }

        var usuario = new CrearUsuarioDto
        {
            NombreUsuario = NombreEntry.Text.Trim(),
            Email = EmailEntry.Text.Trim()
        };

        var tempPass = await _apiService.CrearUsuarioAsync(usuario);

        if (tempPass == null)
        {
            await DisplayAlert("Error", "No se pudo crear el usuario.", "OK");
            return;
        }

        await DisplayAlert("Usuario creado",
            $"Contraseña temporal generada:\n\n{tempPass}\n\nEl usuario deberá cambiarla al iniciar sesión.",
            "OK");

        await Navigation.PopAsync();
    }


}
