using StockWise.Client.Modelo;
using StockWise.Client.Services;

namespace StockWise.Client.Paginas;

public partial class EditarEmpresaPage : ContentPage
{
    private readonly ApiService _api;

    public EditarEmpresaPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var empresa = await _api.ObtenerMiEmpresaAsync();

        if (empresa != null)
        {
            NombreEntry.Text = empresa.Nombre;
            NifEntry.Text = empresa.NIF;
            DireccionEntry.Text = empresa.Direccion;
            EmailEntry.Text = empresa.Email;
            TelefonoEntry.Text = empresa.Telefono;
        }
        else
        {
            await DisplayAlert("Error", "No se pudo cargar la empresa.", "OK");
            await Navigation.PopAsync();
        }
    }

    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        var empresaIdString = await SecureStorage.GetAsync("empresa_id");
        if (string.IsNullOrEmpty(empresaIdString))
        {
            await DisplayAlert("Error", "Empresa no encontrada.", "OK");
            return;
        }

        var empresaDto = new EmpresaDto
        {
            Id = int.Parse(empresaIdString),
            Nombre = NombreEntry.Text?.Trim() ?? "",
            NIF = NifEntry.Text?.Trim() ?? "",
            Direccion = DireccionEntry.Text?.Trim() ?? "",
            Email = EmailEntry.Text?.Trim() ?? "",
            Telefono = TelefonoEntry.Text?.Trim() ?? ""
        };

        bool ok = await _api.ActualizarEmpresaAsync(empresaDto.Id, empresaDto);

        if (ok)
        {
            await DisplayAlert("Éxito", "Datos actualizados correctamente.", "OK");
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Error", "No se pudo actualizar.", "OK");
        }
    }
}
