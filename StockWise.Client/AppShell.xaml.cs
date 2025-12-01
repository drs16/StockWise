using StockWise.Client.Paginas;

namespace StockWise.Client;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(ModificarStockPage), typeof(ModificarStockPage));
        Routing.RegisterRoute("CambiarPassword", typeof(CambiarPassword));
        Routing.RegisterRoute(nameof(RegistroInicialPage), typeof(RegistroInicialPage));
        Routing.RegisterRoute("login", typeof(LoginPage));
        Routing.RegisterRoute("RegistroInicialPage", typeof(RegistroInicialPage));

    }
}
