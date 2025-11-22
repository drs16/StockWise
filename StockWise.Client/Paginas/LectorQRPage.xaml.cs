using ZXing.Net.Maui; // Asegúrate de tener este using para BarcodeDetectionEventArgs

namespace StockWise.Client.Paginas;

public partial class LectorQRPage : ContentPage
{
    public LectorQRPage()
    {
        InitializeComponent();
    }

    private void CameraBarcodeReaderView_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        // Usamos Dispatcher.Dispatch para asegurarnos de que el código de la UI
        // (como DisplayAlert) se ejecute en el hilo principal.
        Dispatcher.Dispatch(async () =>
        {
            // Detenemos la detección inmediatamente para evitar que se sigan leyendo
            // códigos mientras se muestra la alerta o se procesa el resultado.
            if (CameraBarcodeReaderView.IsDetecting)
            {
                CameraBarcodeReaderView.IsDetecting = false;
            }

            // e.Results es una colección, pero normalmente solo tomamos el primero
            var barcode = e.Results?.FirstOrDefault();

            if (barcode != null)
            {
                // El resultado que buscamos está en barcode.Value
                string valorQR = barcode.Value;

                // Muestra el resultado
                await DisplayAlert("Código QR Escaneado", $"El contenido es: {valorQR}", "Aceptar");

                // Opcional: Si quieres volver a la página anterior después de la alerta
                await Shell.Current.GoToAsync("..");
            }
            // Si el código fuera nulo por alguna razón, podríamos reactivar el escaneo
            else
            {
                CameraBarcodeReaderView.IsDetecting = true;
            }
        });
    }

    // Opcional: Implementa OnDisappearing para asegurar que la cámara se detenga cuando la página se cierra.
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Esto libera los recursos de la cámara cuando la página deja de estar visible
        CameraBarcodeReaderView.IsDetecting = false;
    }
}