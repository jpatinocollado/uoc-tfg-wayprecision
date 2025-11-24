using Microsoft.Extensions.Logging;
using WayPrecision.Domain.Data;
using WayPrecision.Domain.Data.UnitOfWork;
using WayPrecision.Domain.Models;
using WayPrecision.Domain.Sensors.Location;
using WayPrecision.Domain.Services;
using WayPrecision.Domain.Services.Configuracion;
using WayPrecision.Domain.Services.Location;
using WayPrecision.Domain.Services.Tracks;
using WayPrecision.Domain.Services.Waypoints;

namespace WayPrecision
{
    // Clase principal para configurar y crear la aplicación MAUI
    public static class MauiProgram
    {
        // Método de entrada para crear y configurar la app MAUI
        public static MauiApp CreateMauiApp()
        {
            // Crea el builder de la aplicación
            var builder = MauiApp.CreateBuilder();
            builder
                // Establece la clase principal de la app
                .UseMauiApp<App>()
                // Configura las fuentes personalizadas
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                // Configura handlers personalizados para WebView según plataforma
                .ConfigureMauiHandlers(handlers =>
                {
#if WINDOWS
                    handlers.AddHandler(typeof(WebView), typeof(CustomWebViewHandler));
#elif ANDROID
                    handlers.AddHandler(typeof(WebView), typeof(CustomWebViewHandler));
#endif
                });

#if DEBUG
            // Habilita el logging de depuración en modo DEBUG
            builder.Logging.AddDebug();
#endif

            // Define la ruta de la base de datos local
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "wayprecision.db3");

            // Lee el contenido SQL inicial si la base de datos no existe
            var sqlContent = string.Empty;
            if (!Path.Exists(dbPath))
            {
                using (var stream = FileSystem.OpenAppPackageFileAsync("wayprecision.db3.sql").Result)
                using (var reader = new StreamReader(stream))
                {
                    sqlContent = reader.ReadToEnd();
                }
            }

            // Registra los servicios en el contenedor de dependencias
            builder.Services.AddScoped(_ => new DatabaseContext(dbPath, sqlContent)); // Contexto de base de datos
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>(); // Patrón UnitOfWork
            builder.Services.AddScoped<IConfigurationService, ConfigurationService>(); // Servicio de configuración
            builder.Services.AddScoped<IService<Waypoint>, WaypointService>(); // Servicio para la gestión de los waypoints
            builder.Services.AddScoped<IService<Track>, TrackService>(); // Servicio para la gestión de los tracks
            builder.Services.AddScoped<InternalGpsManager>(); // Servicio de GPS por sensor
            builder.Services.AddTransient<MockCsvGpsManager>(); // Servicio de GPS por CSV
            builder.Services.AddScoped<GpsManagerFactory>();

            // Construye y retorna la app configurada
            return builder.Build();
        }
    }
}