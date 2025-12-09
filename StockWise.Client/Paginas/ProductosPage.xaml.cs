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
            await DisplayAlert("Error", $"No se pudieron cargar los productos.\n{ex.Message}", "OK");
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
            var confirmar = await DisplayAlert(
                "Importar productos",
                "¿Deseas importar productos desde un archivo CSV?",
                "Sí", "No");

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
                await DisplayAlert("Cancelado", "No se seleccionó ningún archivo.", "OK");
                return;
            }

            var contenido = await File.ReadAllTextAsync(resultado.FullPath);

            var lineas = contenido
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .ToList();

            if (lineas.Count < 2)
            {
                await DisplayAlert("Error", "El archivo CSV está vacío.", "OK");
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
                    EmpresaId = 1
                });
            }

            if (!productos.Any())
            {
                await DisplayAlert("Error", "No se encontraron productos válidos.", "OK");
                return;
            }

            LoadingIndicator.IsVisible = true;

            var ok = await _apiService.ImportarProductosAsync(productos);

            if (ok)
            {
                await DisplayAlert("Éxito", "Productos importados correctamente", "OK");
                await CargarProductosAsync();
            }
            else
            {
                await DisplayAlert("Error", "No se pudieron importar los productos", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnScanearClicked(object sender, EventArgs e)
    {
        await DisplayAlert("QR", "Aquí abrirás el lector de QR", "OK");
    }

    // Cierra el menú (centraliza la lógica)
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

        // Navegar a la página de usuarios (asegúrate de que ListaUsuariosPage tiene un constructor que acepte ApiService)
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
            // Ocultar menú
            await MenuContainer.FadeTo(0, 150);
            MenuContainer.IsVisible = false;
        }
        else
        {
            // Mostrar menú
            MenuContainer.Opacity = 0;
            MenuContainer.IsVisible = true;
            await MenuContainer.FadeTo(1, 150);
        }

        menuVisible = !menuVisible;
    }

    private async void OnPerfilClicked(object sender, EventArgs e)
    {
        await CloseMenu();
        await Navigation.PushAsync(new MiPerfilPage());
    }

    private async Task AplicarPermisosUsuario()
    {
        var rol = await SecureStorage.GetAsync("usuario_rol");

        bool esAdmin = rol?.ToLower() == "administrador";

        // Ocultar botones si no es admin
        BtnCrearProducto.IsVisible = esAdmin;
        BtnImportar.IsVisible = esAdmin;
        BtnAdmin.IsVisible = esAdmin;
        BtnExportar.IsVisible = esAdmin;
        BtnEditarEmpresa.IsVisible = esAdmin; // <- AÑADIDO

    }

    private async void OnExportarClicked(object sender, EventArgs e)
    {
        try
        {
            var empresaId = int.Parse(await SecureStorage.GetAsync("empresa_id"));
            var bytes = await _apiService.ExportarCSVAsync(empresaId);

            if (bytes == null)
            {
                await DisplayAlert("Error", "No se pudo exportar el CSV", "OK");
                return;
            }

            var fileName = "productos.csv";

#if WINDOWS
        // --- EXPORTAR EN WINDOWS PC ---
        var savePicker = new FileSavePicker();

        // Necesario en MAUI WinUI
        var window = App.Current.Windows[0].Handler.PlatformView as MauiWinUIWindow;
        InitializeWithWindow.Initialize(savePicker, window.WindowHandle);

        savePicker.SuggestedFileName = fileName;
        savePicker.FileTypeChoices.Add("CSV File", new List<string>() { ".csv" });

        StorageFile file = await savePicker.PickSaveFileAsync();

        if (file != null)
        {
            using var stream = await file.OpenStreamForWriteAsync();
            stream.Write(bytes, 0, bytes.Length);

            await DisplayAlert("Éxito", $"Archivo guardado en:\n{file.Path}", "OK");
        }
        else
        {
            await DisplayAlert("Cancelado", "No se guardó el archivo.", "OK");
        }
#else
            // --- ANDROID / iOS ---
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
            await DisplayAlert("Error", ex.Message, "OK");
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
