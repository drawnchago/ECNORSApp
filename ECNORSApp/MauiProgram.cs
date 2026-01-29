using ECNORSApp.Services;
using ECNORSAppData.Data.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using static ECNORSApp.Services.CloseLoadApi;
using MudBlazor.Services;

namespace ECNORSApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif
            builder.Services.AddMudServices();

            // ✅ Cargar appsettings.json como MauiAsset (desde el paquete)
            using (var stream = FileSystem.OpenAppPackageFileAsync("appsettings.json")
                                          .GetAwaiter().GetResult())
            {
                // Esto se añade al final, así que "gana" si ya había algo previo
                builder.Configuration.AddJsonStream(stream);
            }

            // UI / Config
            builder.Services.AddSingleton<IConnectionSelector, ConnectionSelector>();
            builder.Services.AddSingleton<SelectedConnectionState>();
            builder.Services.AddSingleton<IToastService, ToastService>();
            builder.Services.AddSingleton<IFileLoggerService, FileLoggerService>();

            // HttpClient base
            var apiBaseUrl = builder.Configuration["Api:BaseUrl"];
            if (string.IsNullOrWhiteSpace(apiBaseUrl))
                throw new InvalidOperationException("Api:BaseUrl no configurado en appsettings.json (Build Action debe ser MauiAsset).");

            var baseUri = new Uri(apiBaseUrl);

            // CloseLoadApi (NO se daña)
            builder.Services.AddHttpClient<CloseLoadApi>(client =>
            {
                client.BaseAddress = baseUri;
                client.Timeout = TimeSpan.FromSeconds(60);
            });

            builder.Services.AddScoped<ICloseLoadApi>(sp => sp.GetRequiredService<CloseLoadApi>());

            // HandbookApi
            builder.Services.AddHttpClient<HandbookApi>(client =>
            {
                client.BaseAddress = baseUri;
                client.Timeout = TimeSpan.FromSeconds(60);
            });

            var app = builder.Build();
            _ = app.Services.GetRequiredService<IFileLoggerService>();

            return app;
        }
    }
}
