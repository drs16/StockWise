using CommunityToolkit.Mvvm.Messaging;
using StockWise.Client.Paginas;
using StockWise.Client.Services;

namespace StockWise.Client;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

#if DEBUG
        Preferences.Clear();
#endif

        MainPage = new AppShell(); // SIEMPRE Shell

        MainPage.Dispatcher.Dispatch(async () =>
        {
            await Task.Delay(100);

            bool registroHecho = Preferences.Get("RegistroInicialCompletado", false);
            bool modoSetup = Preferences.Get("ModoSetup", true);
            // TRUE por defecto la 1ª vez (no existe)

            if (!registroHecho && modoSetup)
            {
                // Primera vez → Setup
                await Shell.Current.GoToAsync("RegistroInicialPage");
                return;
            }

            // Si ya se completó el registro → login
            await Shell.Current.GoToAsync("//login");
        });

        // QR Listener
        WeakReferenceMessenger.Default.Register<QRDetectedMessage>(this, async (r, m) =>
        {
            await ProcesarQRGlobal(m.Value);
        });
    }

    private async Task ProcesarQRGlobal(string qr)
    {
        try
        {
            var api = new ApiService();
            var producto = await api.GetProductoByQRAsync(qr);

            if (producto == null)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    MainPage.DisplayAlert("Error", "Producto no encontrado", "OK")
                );
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Shell.Current.GoToAsync(nameof(ModificarStockPage), true,
                    new Dictionary<string, object> { { "Producto", producto } });
            });
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
                MainPage.DisplayAlert("Error", ex.Message, "OK")
            );
        }
    }
}
