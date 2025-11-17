
namespace StockWise.Client.Paginas;

public partial class MiPerfilPage : ContentPage
{
    public MiPerfilPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        NombreLabel.Text = "Usuario: " + await SecureStorage.GetAsync("usuario_nombre");
        EmailLabel.Text = "Email: " + await SecureStorage.GetAsync("usuario_email");
        RolLabel.Text = "Rol: " + await SecureStorage.GetAsync("usuario_rol");
        EmpresaLabel.Text = "Empresa ID: " + await SecureStorage.GetAsync("empresa_id");
    }
}
