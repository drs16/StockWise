namespace StockWise.Client.Componentes;

public partial class ConfirmacionModalPage : ContentPage
{
    private TaskCompletionSource<bool> _tcs = new();

    public Task<bool> EsperarRespuesta => _tcs.Task;

    public ConfirmacionModalPage(string titulo, string mensaje)
    {
        InitializeComponent();

        TituloLabel.Text = titulo;
        MensajeLabel.Text = mensaje;
    }

    private void SiClicked(object sender, EventArgs e)
    {
        _tcs.TrySetResult(true);
        Navigation.PopModalAsync();
    }

    private void NoClicked(object sender, EventArgs e)
    {
        _tcs.TrySetResult(false);
        Navigation.PopModalAsync();
    }
}
