namespace StockWise.Client.Componentes;

public partial class MensajeModalPage : ContentPage
{
    private TaskCompletionSource<bool> _tcs;

    public Task<bool> EsperarCierre => _tcs.Task;

    public MensajeModalPage(string titulo, string mensaje, string? textoCopiable = null)
    {
        InitializeComponent();
        _tcs = new TaskCompletionSource<bool>();

        TituloLabel.Text = titulo;
        MensajeLabel.Text = mensaje;

        if (!string.IsNullOrEmpty(textoCopiable))
        {
            CopiableEntry.IsVisible = true;
            CopiableEntry.Text = textoCopiable;
        }
    }

    private async void OkClicked(object sender, EventArgs e)
    {
        _tcs.TrySetResult(true);
        await Navigation.PopModalAsync();
    }
}
