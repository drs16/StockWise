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
        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            MessageLabel.Text = "Por favor, rellena todos los campos.";
            return;
        }

        // Muestra un pequeño indicador de carga
        MessageLabel.Text = "Verificando credenciales...";

        var token = await _apiService.LoginAsync(email, password);

        if (token != null)
        {
            await SecureStorage.SetAsync("jwt_token", token);
            _apiService.SetToken(token);

            // 🔹 Aún no hay ProductosPage, así que mostramos mensaje de éxito
            MessageLabel.TextColor = Colors.Green;
            MessageLabel.Text = "Inicio de sesión correcto ✅";

            // Más adelante aquí añadiremos:
             await Navigation.PushAsync(new ProductosPage(_apiService));
        }
        else
        {
            MessageLabel.TextColor = Colors.Red;
            MessageLabel.Text = "Credenciales inválidas.";
        }
    }
}
