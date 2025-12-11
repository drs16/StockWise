using StockWise.Client.Modelo;
using StockWise.Client.Services;
using StockWise.Client.Componentes;

namespace StockWise.Client.Paginas.Usuarios;

public partial class EditarUsuarioPage : ContentPage
{
    private readonly ApiService _apiService;
    UsuarioDto usuario;

    public EditarUsuarioPage(ApiService apiService, UsuarioDto usuarioExistente)
    {
        InitializeComponent();

        _apiService = apiService;
        usuario = usuarioExistente;

        BindingContext = usuario;
    }

    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        usuario.NombreUsuario = NombreEntry.Text.Trim();
        usuario.Email = EmailEntry.Text.Trim();

        var ok = await _apiService.ActualizarUsuarioAsync(usuario);

        if (ok)
        {
            // Mostrar popup de éxito
            var popup = new MensajeModalPage("Éxito", "Usuario modificado con éxito.");
            await Navigation.PushModalAsync(popup);
            await popup.EsperarCierre;   // <-- el modal se cierra aquí

            // 🔥 Espera corta para evitar crash al navegar en Android
            await Task.Delay(50);

            // Volver atrás
            await Navigation.PopAsync();
            return;
        }

        // ERROR
        var popupErr = new MensajeModalPage("Error", "No se pudo modificar el usuario.");
        await Navigation.PushModalAsync(popupErr);
        await popupErr.EsperarCierre;

        await Task.Delay(50);
    }
}
