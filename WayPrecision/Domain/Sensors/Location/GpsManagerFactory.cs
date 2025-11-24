using WayPrecision.Domain.Services.Configuracion;
using WayPrecision.Domain.Services.Location;

namespace WayPrecision.Domain.Sensors.Location
{
    public class GpsManagerFactory(IServiceProvider sp)
    {
        private readonly IServiceProvider _sp = sp;

        public async Task<IGpsManager> Create()
        {
            IConfigurationService configurationService = _sp.GetRequiredService<IConfigurationService>();
            var configuration = await configurationService.GetOrCreateAsync();

            IGpsManager gpsManager = configuration.TrackingMode switch
            {
                "CSV" => _sp.GetRequiredService<MockCsvGpsManager>(),
                _ => _sp.GetRequiredService<InternalGpsManager>(),
            };

            await gpsManager.ChangeGpsInterval(new TimeSpan(0, 0, configuration.GpsInterval));

            return gpsManager;
        }
    }
}