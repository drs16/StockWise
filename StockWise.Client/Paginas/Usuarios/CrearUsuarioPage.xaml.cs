using StockWise.Client.Modelo;
using StockWise.Client.Services;

namespace StockWise.Client.Paginas.Usuarios;

public partial class CrearUsuarioPage : ContentPage
{
    private readonly ApiService _apiService;
    private int empresaId;

    public CrearUsuarioPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        empresaId = int.Parse(await SecureStorage.GetAsync("empresa_id"));
    }

    private async void OnCrearClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NombreEntry.Text) ||
            string.IsNullOrWhiteSpace(EmailEntry.Text) ||
            string.IsNullOrWhiteSpace(PasswordEntry.Text) ||
            RolPicker.SelectedItem == null)
        {
            await DisplayAlert("Error", "Por favor completa todos los campos.", "OK");
            return;
        }

        var usuario = new UsuarioDto
        {
            NombreUsuario = NombreEntry.Text.Trim(),
            Email = EmailEntry.Text.Trim(),
            PasswordHash = PasswordEntry.Text.Trim(),  // 🔥 SE MANDA LA CONTRASEÑA NORMAL
            Rol = RolPicker.SelectedItem.ToString(),
            EmpresaId = empresaId
        };

        var ok = await _apiService.CrearUsuarioAsync(usuario);

        if (ok)
        {
            await DisplayAlert("Éxito", "Usuario creado correctamente.", "OK");
            await Navigation.PopAsync(); // volver a lista
        }
        else
        {
            await DisplayAlert("Error", "No se pudo crear el usuario.", "OK");
        }
    }
}
