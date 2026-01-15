using ECNORSApp.Services;
using ECNORSAppData.Data.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using static ECNORSApp.Services.CloseLoadApi;

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

            using (var stream = FileSystem.OpenAppPackageFileAsync("appsettings.json")
                                          .GetAwaiter().GetResult())
            {
                builder.Configuration.AddJsonStream(stream);
            }

            // UI / Config
            builder.Services.AddSingleton<IConnectionSelector, ConnectionSelector>();
            builder.Services.AddSingleton<SelectedConnectionState>();
            builder.Services.AddSingleton<IToastService, ToastService>();
            builder.Services.AddSingleton<IFileLoggerService, FileLoggerService>();

            // HttpClient (typed client)
            builder.Services.AddHttpClient<CloseLoadApi>(client =>
            {
                client.BaseAddress = new Uri("http://172.20.11.85:5090/");
                client.Timeout = TimeSpan.FromSeconds(60);
            });

            // Interfaz -> implementación (una sola vez)
            builder.Services.AddScoped<ICloseLoadApi>(sp => sp.GetRequiredService<CloseLoadApi>());

            var app = builder.Build();
            _ = app.Services.GetRequiredService<IFileLoggerService>();

            return app;
        }
    }
}
