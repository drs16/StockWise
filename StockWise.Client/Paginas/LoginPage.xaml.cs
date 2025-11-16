using StockWise.Client.Services;
using System.IdentityModel.Tokens.Jwt;

namespace StockWise.Client.Paginas;

public partial class LoginPage : ContentPage
{
    private readonly ApiService _apiService;

    public LoginPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        MessageLabel.Text = "";
        var email = EmailEntry.Text;
        var password = PasswordEntry.Text;

        // 👉 Login devuelve solo el token
        var token = await _apiService.LoginAsync(email, password);

        if (string.IsNullOrEmpty(token))
        {
            MessageLabel.Text = "Credenciales inválidas.";
            return;
        }

        // Guardar token
        await SecureStorage.SetAsync("jwt_token", token);
        _apiService.SetToken(token);

        // 👉 EXTRAER EmpresaId DEL TOKEN
        string empresaId = ObtenerClaim(token, "EmpresaId");

        if (string.IsNullOrEmpty(empresaId))
        {
            await DisplayAlert("Error", "No se pudo obtener EmpresaId del token.", "OK");
            return;
        }

        // Guardar empresa_id
        await SecureStorage.SetAsync("empresa_id", empresaId);

        await DisplayAlert("Éxito", "Inicio de sesión correcto", "OK");

        // Navegar
        await Shell.Current.GoToAsync("//productos");
    }

    public string ObtenerClaim(string token, string claimType)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        return jwt.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
    }
}
