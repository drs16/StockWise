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

        // Guardar token (siempre no nulo)
        await SecureStorage.SetAsync("jwt_token", token);

        _apiService.SetToken(token);


        // Extraer claims de forma robusta
        string GetClaim(string t, string claimTypeOrPartial)
        {
            if (string.IsNullOrEmpty(t)) return "";
            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(t);
                // Primero buscar claim exacto (EmpresaId)
                var exact = jwt.Claims.FirstOrDefault(c => c.Type == claimTypeOrPartial);
                if (exact != null) return exact.Value;
                // Luego buscar por sufijo/substring común (name, role)
                var partial = jwt.Claims.FirstOrDefault(c => c.Type != null && c.Type.Contains(claimTypeOrPartial));
                return partial?.Value ?? "";
            }
            catch
            {
                return "";
            }
        }

        var empresaId = GetClaim(token, "EmpresaId") ?? "";
        var rol = GetClaim(token, "role");
        var nombre = GetClaim(token, "name"); // puede venir como esquema completo
                                              // nombre o email: si nombre vacío, usamos el email que ya conocemos
        var usuarioNombre = string.IsNullOrEmpty(nombre) ? email : nombre;
        var debeCambiar = GetClaim(token, "DebeCambiarPassword") ?? "false";
        await SecureStorage.SetAsync("debe_cambiar", debeCambiar);
        // Guardar seguros (nunca pasar null)
        await SecureStorage.SetAsync("empresa_id", empresaId ?? "");
        await SecureStorage.SetAsync("usuario_rol", rol ?? "");
        await SecureStorage.SetAsync("usuario_nombre", usuarioNombre ?? "");
        await SecureStorage.SetAsync("usuario_email", email ?? "");

        if (string.IsNullOrEmpty(empresaId))
        {
            await DisplayAlert("Aviso", "EmpresaId no se encontró en el token. Comprueba el token o el servidor.", "OK");
            // Puedes elegir fallar aquí o seguir navegando; yo solo aviso.
        }

        await DisplayAlert("Éxito", "Inicio de sesión correcto", "OK");

        var flag = await SecureStorage.GetAsync("debe_cambiar");

        if (flag == "True")
        {
            await DisplayAlert("Aviso", "Debes cambiar tu contraseña antes de continuar.", "OK");
            await Shell.Current.GoToAsync("//CambiarPassword");
            return;
        }


        // Navegar (ya guardamos rol)
        if ((rol ?? "").ToLower() != "administrador")
            await Shell.Current.GoToAsync("//productos");
        else
            await Shell.Current.GoToAsync("//productos");
    }


    public string ObtenerClaim(string token, string claimType)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        return jwt.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
    }

    private async void OnRegistrarEmpresaClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("RegistroInicialPage");
    }

}