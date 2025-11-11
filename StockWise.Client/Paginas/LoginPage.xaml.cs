using StockWise.Client.Services;

namespace StockWise.Client.Paginas;

public partial class LoginPage : ContentPage
{
    private readonly ApiService _apiService = new();

    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        MessageLabel.Text = "";
        var email = EmailEntry.Text;
        var password = PasswordEntry.Text;

        var token = await _apiService.LoginAsync(email, password);

        if (token != null)
        {
            await SecureStorage.SetAsync("jwt_token", token);
            _apiService.SetToken(token);

            await DisplayAlert("Éxito", "Inicio de sesión correcto", "OK");
            // Aquí más adelante navegaremos a ProductosPage
        }
        else
        {
            MessageLabel.Text = "Credenciales inválidas.";
        }
    }
}
