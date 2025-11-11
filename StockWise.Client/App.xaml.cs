using StockWise.Client.Paginas;

namespace StockWise.Client;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell(); // 🟢 Control de navegación
    }
}
