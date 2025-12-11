using StockWise.Client.Componentes;
using StockWise.Client.Modelo;
using StockWise.Client.Services;
using Microsoft.Maui.ApplicationModel.DataTransfer;

#if WINDOWS
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
using Microsoft.UI.Xaml;
using Microsoft.Maui.Platform;
#endif

namespace StockWise.Client.Paginas;

public partial class ProductosPage : ContentPage
{
    private readonly ApiService _apiService;
    private bool menuVisible = false;

    public ProductosPage()
    {
        InitializeComponent();
        _apiService = new ApiService();

#if ANDROID
        BtnQR.IsVisible = true;
#else
        BtnQR.IsVisible = false;
#endif
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await AplicarPermisosUsuario();
        await CargarProductosAsync();
    }

    /// <summary>
    /// Muestra un popup modal personalizado con botón OK.
    /// </summary>
    private async Task MostrarPopup(string titulo, string mensaje)
    {
        var popup = new MensajeModalPage(titulo, mensaje);
        await Navigation.PushModalAsync(popup);
        await popup.EsperarCierre;
    }

    private async Task CargarProductosAsync()
    {
        try
        {
            var token = await SecureStorage.GetAsync("jwt_token");

            if (!string.IsNullOrEmpty(token))
                _apiService.SetToken(token);

            LoadingIndicator.IsVisible = true;

            var productos = await _apiService.GetProductosAsync();

            if (productos == null || !productos.Any())
            {
                EmptyState.IsVisible = true;
                ProductosList.IsVisible = false;
            }
            else
            {
                ProductosList.ItemsSource = productos;
                ProductosList.IsVisible = true;
                EmptyState.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            await MostrarPopup("Error", $"No se pudieron cargar los productos.\n{ex.Message}");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void ProductosList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is ProductoDto producto)
        {
            await Navigation.PushAsync(new ProductoDetallePage(producto, _apiService));
            ((CollectionView)sender).SelectedItem = null;
        }
    }

    private async Task LogoutAsync()
    {
        SecureStorage.Remove("jwt_token");
        await Shell.Current.GoToAsync("//login");
    }

    private async void OnCrearProductoClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CrearProductoPage(_apiService));
    }

    private async void OnImportarClicked(object sender, EventArgs e)
    {
        try
        {
            var popupConfirm = new ConfirmacionModalPage(
                "Importar productos",
                "¿Deseas importar productos desde un archivo CSV?"
            );

            await Navigation.PushModalAsync(popupConfirm);
            bool confirmar = await popupConfirm.EsperarRespuesta;

            if (!confirmar)
                return;

            var csvFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, new[] { ".csv" } },
                { DevicePlatform.Android, new[] { "text/csv" } },
                { DevicePlatform.iOS, new[] { "public.comma-separated-values-text" } },
                { DevicePlatform.MacCatalyst, new[] { "public.comma-separated-values-text" } }
            });

            var resultado = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Selecciona el archivo CSV",
                FileTypes = csvFileType
            });

            if (resultado == null)
            {
                await MostrarPopup("Cancelado", "No se seleccionó ningún archivo.");
                return;
            }

            var contenido = await File.ReadAllTextAsync(resultado.FullPath);

            var lineas = contenido
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .ToList();

            if (lineas.Count < 2)
            {
                await MostrarPopup("Error", "El archivo CSV está vacío.");
                return;
            }

            var encabezado = lineas[0].ToLower();
            bool tieneQR = encabezado.Contains("codigoqr");

            var productos = new List<ProductoDto>();

            foreach (var linea in lineas.Skip(1))
            {
                var campos = linea.Split(',');

                if (campos.Length < 4)
                    continue;

                string nombre = campos[0].Trim();
                string proveedor = campos[1].Trim();
                int cantidad = int.TryParse(campos[2], out var c) ? c : 0;
                decimal precio = decimal.TryParse(campos[3], out var p) ? p : 0;

                string codigoQR =
                    tieneQR && campos.Length > 4 && !string.IsNullOrWhiteSpace(campos[4])
                    ? campos[4].Trim()
                    : $"{nombre[..Math.Min(4, nombre.Length)].ToUpper()}-{Guid.NewGuid().ToString()[..6]}";

                productos.Add(new ProductoDto
                {
                    Nombre = nombre,
                    Proveedor = proveedor,
                    Cantidad = cantidad,
                    Precio = precio,
                    CodigoQR = codigoQR,
                    EmpresaId = int.Parse(await SecureStorage.GetAsync("empresa_id"))
                });
            }

            if (!productos.Any())
            {
                await MostrarPopup("Error", "No se encontraron productos válidos.");
                return;
            }

            LoadingIndicator.IsVisible = true;

            var ok = await _apiService.ImportarProductosAsync(productos);

            if (ok)
            {
                await MostrarPopup("Éxito", "Productos importados correctamente.");
                await CargarProductosAsync();
            }
            else
            {
                await MostrarPopup("Error", "No se pudieron importar los productos.");
            }
        }
        catch (Exception ex)
        {
            await MostrarPopup("Error", ex.Message);
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnScanearClicked(object sender, EventArgs e)
    {
        await MostrarPopup("QR", "Aquí abrirás el lector de QR.");
    }

    private async Task CloseMenu()
    {
        if (menuVisible)
        {
            await MenuContainer.FadeTo(0, 120);
            MenuContainer.IsVisible = false;
            menuVisible = false;
        }
    }

    private async void OnAdminClicked(object sender, EventArgs e)
    {
        await CloseMenu();
        await Navigation.PushAsync(new Paginas.Usuarios.ListaUsuariosPage(_apiService));
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await CloseMenu();
        await LogoutAsync();
    }

    private async void OnMenuToggleClicked(object sender, EventArgs e)
    {
        if (menuVisible)
        {
            await MenuContainer.FadeTo(0, 150);
            MenuContainer.IsVisible = false;
        }
        else
        {
            MenuContainer.Opacity = 0;
            MenuContainer.IsVisible = true;
            await MenuContainer.FadeTo(1, 150);
        }

        menuVisible = !menuVisible;
    }

    private async void OnPerfilClicked(object sender, EventArgs e)
    {
        await CloseMenu();
        await Navigation.PushAsync(new MiPerfilPage(_apiService));
    }

    private async Task AplicarPermisosUsuario()
    {
        var rol = await SecureStorage.GetAsync("usuario_rol");

        bool esAdmin = rol?.ToLower() == "administrador";

        BtnCrearProducto.IsVisible = esAdmin;
        BtnImportar.IsVisible = esAdmin;
        BtnAdmin.IsVisible = esAdmin;
        BtnExportar.IsVisible = esAdmin;
        BtnEditarEmpresa.IsVisible = esAdmin;
    }

    private async void OnExportarClicked(object sender, EventArgs e)
    {
        try
        {
            var empresaId = int.Parse(await SecureStorage.GetAsync("empresa_id"));
            var bytes = await _apiService.ExportarCSVAsync(empresaId);

            if (bytes == null)
            {
                await MostrarPopup("Error", "No se pudo exportar el CSV.");
                return;
            }

            var fileName = "productos.csv";

#if WINDOWS
            var savePicker = new FileSavePicker();
            var window = App.Current.Windows[0].Handler.PlatformView as MauiWinUIWindow;
            InitializeWithWindow.Initialize(savePicker, window.WindowHandle);

            savePicker.SuggestedFileName = fileName;
            savePicker.FileTypeChoices.Add("CSV File", new List<string>() { ".csv" });

            StorageFile file = await savePicker.PickSaveFileAsync();

            if (file != null)
            {
                using var stream = await file.OpenStreamForWriteAsync();
                stream.Write(bytes, 0, bytes.Length);

                await MostrarPopup("Éxito", $"Archivo guardado en:\n{file.Path}");
            }
            else
            {
                await MostrarPopup("Cancelado", "No se guardó el archivo.");
            }
#else
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
            File.WriteAllBytes(filePath, bytes);

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Exportar productos",
                File = new ShareFile(filePath)
            });
#endif
        }
        catch (Exception ex)
        {
            await MostrarPopup("Error", ex.Message);
        }
    }

    private async void OnEditarEmpresaClicked(object sender, EventArgs e)
    {
        await CloseMenu();
        await Navigation.PushAsync(new EditarEmpresaPage(_apiService));
    }

    private async void AbrirLectorQR(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LectorQRPage());
    }

    private async void OnProductoTapped(object sender, EventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is ProductoDto producto)
        {
            await Navigation.PushAsync(new ProductoDetallePage(producto, _apiService));
        }
    }

    private async void OnMovimientosClicked(object sender, EventArgs e)
    {
        await CloseMenu();
        await Navigation.PushAsync(new MovimientoStockPage(_apiService));
    }
}
