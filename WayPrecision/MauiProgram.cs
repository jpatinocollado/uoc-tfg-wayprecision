using Microsoft.Extensions.Logging;
using WayPrecision.Domain.Data;

namespace WayPrecision
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
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .ConfigureMauiHandlers(handlers =>
                {
#if WINDOWS
                handlers.AddHandler(typeof(WebView), typeof(CustomWebViewHandler));
#elif ANDROID
                    handlers.AddHandler(typeof(WebView), typeof(CustomWebViewHandler));
#endif
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "wayprecision.db3");

            builder.Services.AddSingleton(new DatabaseContext(dbPath));
            builder.Services.AddSingleton<IUnitOfWork, UnitOfWork>();

            return builder.Build();
        }
    }
}