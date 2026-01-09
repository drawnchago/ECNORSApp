using ECNORSAppData.Data;
using ECNORSAppData.Data.Config;
using ECNORSAppData.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;

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

            builder.Services.AddSingleton<IConnectionSelector, ConnectionSelector>();
            builder.Services.AddSingleton<SelectedConnectionState>();

            builder.Services.AddDbContextFactory<AppDbContext>((sp, opt) =>
            {
                var selector = sp.GetRequiredService<IConnectionSelector>();
                var state = sp.GetRequiredService<SelectedConnectionState>();

                var all = selector.GetConnections();
                var selectedName = state.GetSelectedName();
                var item = all.FirstOrDefault(x => x.name == selectedName) ?? all.FirstOrDefault();

                var cs = item is not null
                    ? selector.BuildConnectionString(item)
                    : builder.Configuration.GetConnectionString("DefaultConnection");

                opt.UseSqlServer(cs);
            });

            builder.Services.AddScoped<ICloseLoadService, CloseLoadService>();
            return builder.Build();
        }
    }
}
