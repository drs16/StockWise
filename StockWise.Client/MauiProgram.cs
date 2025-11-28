// 1. Añade el using del Toolkit
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using StockWise.Client.Services;
using ZXing.Net.Maui.Controls;

namespace StockWise.Client;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            // 2. Coloca la inicialización del Toolkit aquí (antes de ConfigureFonts)
            .UseMauiCommunityToolkit()

            // Inicializa ZXing.Net.Maui
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            // El resto de tus inicializaciones
            .UseBarcodeReader();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<ApiService>();

        return builder.Build();
    }
}