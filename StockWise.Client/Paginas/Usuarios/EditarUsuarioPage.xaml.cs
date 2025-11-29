using StockWise.Client.Modelo;
using StockWise.Client.Services;

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
            await DisplayAlert("Éxito", "Usuario modificado con éxito.", "OK");
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Error", "No se pudo modificar el usuario.", "OK");
        }
    }
}
