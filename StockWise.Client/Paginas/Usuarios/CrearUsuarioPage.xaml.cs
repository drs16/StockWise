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
        await Task.Delay(80);
        await Navigation.PopModalAsync();
        return;
    }

    var usuario = new CrearUsuarioDto
    {
        NombreUsuario = NombreEntry.Text.Trim(),
        Email = EmailEntry.Text.Trim()
    };

        var tempPass = await _apiService.CrearUsuarioAsync(usuario);

        if (tempPass.StartsWith("ERROR:"))
        {
            string mensaje = tempPass.Replace("ERROR:", "");

            var popup = new MensajeModalPage("Error", mensaje);
            await Navigation.PushModalAsync(popup);
            await popup.EsperarCierre;
            return;
        }


        var popupOk = new MensajeModalPage(
        "Usuario creado",
        "Se generó la siguiente contraseña temporal:",
        tempPass
    );

        await Navigation.PushModalAsync(popupOk);

        // Esperar a que el usuario pulse OK en el popup
        await popupOk.EsperarCierre;

        // Esperar un instante para evitar conflictos de navegación
        await Task.Delay(80);

   

        // Regresamos a la lista de usuarios
        await Navigation.PopAsync();

    }

}
