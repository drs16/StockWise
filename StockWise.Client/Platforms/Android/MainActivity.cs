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

            Toast.MakeText(this, "QR externo: " + resultado, ToastLength.Long).Show();

            // 👇 NAVEGAR DESDE ANDROID -> MAUI
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var app = Microsoft.Maui.Controls.Application.Current as App;

                if (app?.MainPage != null)
                {
                    await app.MainPage.Navigation.PushAsync(new ModificarStockPage(resultado));
                }
            });

        }
    }

}
