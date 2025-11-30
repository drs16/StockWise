using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Dispatching;
using StockWise.Client.Paginas;
using StockWise.Client.Services;

namespace StockWise.Client;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // ⭐ MODO DEBUG PARA PROBAR REGISTRO INICIAL SIN SEPARAR APKs ⭐
        bool DEBUG_SETUP = true; // Cámbialo a false cuando quieras probar modo normal

        if (DEBUG_SETUP)
        {
            // Simular que esta APK es modo setup
            Preferences.Set("ModoSetup", true);
        }

        // 1️⃣ SI YA SE COMPLETÓ EL REGISTRO INICIAL → ENTRAR NORMAL
        if (Preferences.Get("RegistroInicialCompletado", false))
        {
            MainPage = new AppShell();
        }
        else
        {
            // 2️⃣ SI ESTA APK ES MODO SETUP → MOSTRAR REGISTRO
            if (Preferences.Get("ModoSetup", false))
            {
                MainPage = new RegistroInicialPage();
            }
            else
            {
                // 3️⃣ APP NORMAL (DESCARGADA DE QR NORMAL)
                MainPage = new AppShell();
            }
        }

        // 4️⃣ SEGUIR ESCUCHANDO QR DE PRODUCTOS (MANTIENE TU LÓGICA ORIGINAL)
        WeakReferenceMessenger.Default.Register<QRDetectedMessage>(this, async (r, m) =>
        {
            await ProcesarQRGlobal(m.Value);
        });
    }

    // 🚀 PROCESAR QR DE PRODUCTOS (TU LÓGICA ORIGINAL)
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
