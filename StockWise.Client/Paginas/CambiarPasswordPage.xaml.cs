using StockWise.Client.Services;

namespace StockWise.Client.Paginas;

public partial class CambiarPassword : ContentPage
{
    private readonly ApiService _apiService;
    public CambiarPassword(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }
    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        if (Nueva1Entry.Text != Nueva2Entry.Text)
        {
            await DisplayAlert("Error", "Las contraseñas no coinciden.", "OK");
            return;
        }

        var ok = await _apiService.CambiarMiPassword(Nueva1Entry.Text);

        if (ok)
        {
            await DisplayAlert("Éxito", "Contraseña actualizada.", "OK");

            // eliminar flag de cambiar contraseña
            await SecureStorage.SetAsync("debe_cambiar", "false");

            await Shell.Current.GoToAsync("//productos");
        }
        else
            await DisplayAlert("Error", "No se pudo actualizar.", "OK");
    }


}