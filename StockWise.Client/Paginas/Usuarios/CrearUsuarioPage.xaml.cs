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
            string.IsNullOrWhiteSpace(EmailEntry.Text) ||
            string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            await DisplayAlert("Error", "Por favor completa todos los campos.", "OK");
            return;
        }

        var usuario = new CrearUsuarioDto
        {
            NombreUsuario = NombreEntry.Text.Trim(),
            Email = EmailEntry.Text.Trim(),
            Password = PasswordEntry.Text.Trim()
        };

        var ok = await _apiService.CrearUsuarioAsync(usuario);

        if (ok)
        {
            await DisplayAlert("Éxito", "Usuario creado correctamente.", "OK");
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Error", "No se pudo crear el usuario.", "OK");
        }
    }
}
