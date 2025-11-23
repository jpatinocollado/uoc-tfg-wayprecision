using WayPrecision.Domain.Data.UnitOfWork;
using WayPrecision.Domain.Models;

namespace WayPrecision.Domain.Services.Configuracion
{
    /// <summary>
    /// Servicio encargado de gestionar la configuración global de la aplicación.
    /// </summary>
    public class ConfigurationService : IConfigurationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ConfigurationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Obtiene la configuración almacenada o crea una por defecto si no existe.
        /// </summary>
        public async Task<Configuration> GetOrCreateAsync()
        {
            // Obtener el único registro existente
            var configs = await _unitOfWork.Configurations.GetAllAsync();
            var config = configs.FirstOrDefault();

            if (config == null)
            {
                config = CreateDefaultConfiguration();

                // Guardamos en la base de datos
                await _unitOfWork.Configurations.InsertAsync(config);
            }

            return await Task.FromResult(config);
        }

        /// <summary>
        /// Actualiza la configuración existente en la base de datos.
        /// </summary>
        public async Task SaveAsync(Configuration updatedConfig)
        {
            if (updatedConfig == null)
                throw new ArgumentNullException(nameof(updatedConfig));

            _unitOfWork.Configurations.UpdateDeferred(updatedConfig);

            await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Crea una configuración inicial con valores por defecto.
        /// </summary>
        private Configuration CreateDefaultConfiguration()
        {
            return new Configuration
            {
                Guid = Guid.NewGuid().ToString(),
                AreaUnits = UnitEnum.MetrosCuadrados.ToString(),
                LengthUnits = UnitEnum.Metros.ToString(),
                GpsInterval = 3,
                GpsAccuracy = 10,
                MovingAverageFilterEnabled = true,
                OutliersFilterEnabled = true,
                KalmanFilterEnabled = true,
                TrackingMode = TrackingModeEnum.GPS.ToString()
            };
        }
    }
}