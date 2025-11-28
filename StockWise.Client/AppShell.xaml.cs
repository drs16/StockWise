using StockWise.Client.Paginas;

namespace StockWise.Client;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(ModificarStockPage), typeof(ModificarStockPage));

    }
}
