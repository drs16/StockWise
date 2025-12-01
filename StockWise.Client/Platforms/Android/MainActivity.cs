using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using CommunityToolkit.Mvvm.Messaging;  // Messenger
using Microsoft.Maui.ApplicationModel;   // MainThread
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using StockWise.Client.Paginas;
using Android.App;


namespace StockWise.Client;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize
                         | ConfigChanges.Orientation
                         | ConfigChanges.UiMode
                         | ConfigChanges.ScreenLayout
                         | ConfigChanges.SmallestScreenSize
                         | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
    {
        base.OnActivityResult(requestCode, resultCode, data);

        if (resultCode == Result.Ok && data != null)
        {
            string resultado = data.GetStringExtra("SCAN_RESULT") ?? string.Empty;
            resultado = resultado.Trim().Replace("\n", "").Replace("\r", "").Replace(" ", "");

            Toast.MakeText(this, "QR externo: " + resultado, ToastLength.Short).Show();

            // Usamos BeginInvokeOnMainThread para seguridad
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    // Intentamos la navegación Shell (preferida)
                    if (Shell.Current != null)
                    {
                        await Shell.Current.GoToAsync($"ModificarStockPage?qr={Uri.EscapeDataString(resultado)}");
                        return;
                    }

                    // Si Shell no existe, intentamos enviar al messenger (fallback)
                    WeakReferenceMessenger.Default.Send(new QRDetectedMessage(resultado));
                }
                catch (Exception ex)
                {
                    // Fallback: enviar por messenger para que la page lo coja
                    WeakReferenceMessenger.Default.Send(new QRDetectedMessage(resultado));

                    // Mostrar el error para que puedas verlo cuando no depuras
                    Toast.MakeText(this, "Navegation error: " + ex.Message, ToastLength.Long).Show();

                    // opcional: log a Android log
                    Android.Util.Log.Error("MainActivity", ex.ToString());
                }
            });
        }
    }
}
