namespace StockWise.Client.Controls;

public class CameraPreview : View
{
    public event EventHandler<string>? OnQrDetected;

    public void RaiseQRCode(string value)
    {
        OnQrDetected?.Invoke(this, value);
    }
}
