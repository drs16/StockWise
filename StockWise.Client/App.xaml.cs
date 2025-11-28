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
        MainPage = new AppShell();

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

            // 🚀 Navegar SIEMPRE desde el hilo principal
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Shell.Current.GoToAsync(nameof(ModificarStockPage), true, new Dictionary<string, object>
                {
                    { "Producto", producto }
                });
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
