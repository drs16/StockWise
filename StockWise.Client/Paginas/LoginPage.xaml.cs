using StockWise.Client.Componentes;
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
        try
        {
            MessageLabel.Text = "";
        var email = EmailEntry.Text;
        var password = PasswordEntry.Text;

        // 👉 Login devuelve solo el token
        var token = await _apiService.LoginAsync(email, password);

        if (string.IsNullOrEmpty(token))
        {
            var popupError = new MensajeModalPage("Error", "Credenciales inválidas.");
            await Navigation.PushModalAsync(popupError);
            await popupError.EsperarCierre;
            return;
        }

        // Guardar token
        await SecureStorage.SetAsync("jwt_token", token);
        _apiService.SetToken(token);

        // FUNCION AUXILIAR PARA LEER CLAIMS
        string GetClaim(string t, string claimTypeOrPartial)
        {
            if (string.IsNullOrEmpty(t)) return "";
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(t);

                var exact = jwt.Claims.FirstOrDefault(c => c.Type == claimTypeOrPartial);
                if (exact != null) return exact.Value;

                var partial = jwt.Claims.FirstOrDefault(c => c.Type.Contains(claimTypeOrPartial));
                return partial?.Value ?? "";
            }
            catch { return ""; }
        }

        var empresaId = GetClaim(token, "EmpresaId");
        var rol = GetClaim(token, "role");
        var nombre = GetClaim(token, "name");
        var debeCambiar = GetClaim(token, "DebeCambiarPassword") ?? "false";

        var usuarioNombre = string.IsNullOrEmpty(nombre) ? email : nombre;

        // GUARDAR DATOS EN SECURE STORAGE
        await SecureStorage.SetAsync("empresa_id", empresaId ?? "");
        await SecureStorage.SetAsync("usuario_rol", rol ?? "");
        await SecureStorage.SetAsync("usuario_nombre", usuarioNombre ?? "");
        await SecureStorage.SetAsync("usuario_email", email ?? "");
        await SecureStorage.SetAsync("debe_cambiar", debeCambiar);

            var popup = new MensajeModalPage("Éxito", "Inicio de sesión correcto");

            // Mostrar modal
            await App.Current.MainPage.Navigation.PushModalAsync(popup);

            // Esperar cierre SIN bloquear el hilo principal
            var result = await popup.EsperarCierre.ConfigureAwait(false);

            // Volver al hilo principal ANTES de navegar
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                // Aquí ya es SEGURO hacer navegación
                await ContinuarDespuesDelLogin();
            });


            // SI EL USUARIO DEBE CAMBIAR CONTRASEÑA
            if (debeCambiar == "True")
        {
            var popupCambiar = new MensajeModalPage("Aviso",
                "Debes cambiar tu contraseña antes de continuar.");
            await Navigation.PushModalAsync(popupCambiar);
            await popupCambiar.EsperarCierre;

            await Shell.Current.GoToAsync("//CambiarPassword");
            return;
        }

        // NAVEGACIÓN SEGÚN ROL
        await Shell.Current.GoToAsync("//productos");
    }
    catch (Exception ex)
    {
        await DisplayAlert("ERROR ANDROID", ex.ToString(), "OK");
}
    }

    private async void OnRegistrarEmpresaClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("RegistroInicialPage");
    }

    private async Task ContinuarDespuesDelLogin()
    {
        var flag = await SecureStorage.GetAsync("debe_cambiar");

        if (flag == "True")
        {
            await Shell.Current.GoToAsync("//CambiarPassword");
            return;
        }

        var rol = await SecureStorage.GetAsync("usuario_rol");

        await Shell.Current.GoToAsync("//productos");
    }


}
