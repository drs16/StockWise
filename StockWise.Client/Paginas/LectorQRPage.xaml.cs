#if ANDROID
using Android.Content;
#endif

using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using StockWise.Client.Services;
using ZXing.Net.Maui;
using CommunityToolkit.Mvvm.Messaging;


namespace StockWise.Client.Paginas
{
    public partial class LectorQRPage : ContentPage
    {
        private bool _isProcessing = false;

        public LectorQRPage()
        {
            InitializeComponent();
            WeakReferenceMessenger.Default.Register<QRDetectedMessage>(this, async (r, m) =>
        {
            if (!_isProcessing)
                await ProcesarQR(m.Value);
        });
        }

        // ESCANER INTERNO
        private void barcodeReader_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var result = e.Results?.FirstOrDefault();
                if (result != null && !_isProcessing)
                {
                    await ProcesarQR(result.Value);
                }
            });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Aquí no hacemos nada ya
        }

        // ESCÁNER EXTERNO
        private async void AbrirScannerExterno(object sender, EventArgs e)
        {
#if ANDROID
            try
            {
                var intent = new Intent("com.google.zxing.client.android.SCAN");
                intent.PutExtra("SCAN_MODE", "QR_CODE_MODE");

                Platform.CurrentActivity.StartActivityForResult(intent, 0);
            }
            catch
            {
                await DisplayAlert("Aviso", "No hay apps de escaneo instaladas.", "OK");
            }
#else
            await DisplayAlert("Aviso", "Disponible solo en Android.", "OK");
#endif
        }

        // 🔥 AL VOLVER A LA APP DESPUÉS DEL ESCÁNER EXTERNO
        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);

            if (_isProcessing)
                return;

            var pending = Preferences.Get("LastExternalQR", null as string);

            if (!string.IsNullOrWhiteSpace(pending))
            {
                Preferences.Remove("LastExternalQR");

                _ = ProcesarQR(pending);
            }
        }

        // PROCESAR QR PARA AMBOS METODOS
        private async Task ProcesarQR(string qr)
        {
            if (_isProcessing) return;
            _isProcessing = true;

            try
            {
                await Shell.Current.GoToAsync($"ModificarStockPage?qr={Uri.EscapeDataString(qr)}");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Navegación fallida: " + ex.Message, "OK");
            }
            finally
            {
                await Task.Delay(300);
                _isProcessing = false;
            }
        }
    }
    }
