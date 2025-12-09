using StockWise.Client.Modelo;
using StockWise.Client.Services;
using StockWise.Client.Componentes;

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
            var popup = new MensajeModalPage("Error", "Por favor completa todos los campos.");
            await Navigation.PushModalAsync(popup);
            await popup.EsperarCierre;
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
            var popup = new MensajeModalPage("Error", "No se pudo crear el usuario.");
            await Navigation.PushModalAsync(popup);
            await popup.EsperarCierre;
            return;
        }

        // Popup con contraseña copiable
        var popupOk = new MensajeModalPage(
            "Usuario creado",
            "Se generó la siguiente contraseña temporal:",
            tempPass
        );

        await Navigation.PushModalAsync(popupOk);
        await popupOk.EsperarCierre;

        await Navigation.PopAsync();
    }
}
