using Microsoft.Extensions.Logging;
using WayPrecision.Domain.Data;
using WayPrecision.Domain.Data.UnitOfWork;
using WayPrecision.Domain.Models;
using WayPrecision.Domain.Sensors.Location;
using WayPrecision.Domain.Services;

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

            builder.Services.AddScoped(_ => new DatabaseContext(dbPath));
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IGpsManager, MockGpsManager>();
            builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
            builder.Services.AddScoped<IService<Waypoint>, WaypointService>();
            builder.Services.AddSingleton<IService<Track>, TrackService>();

            return builder.Build();
        }
    }
}