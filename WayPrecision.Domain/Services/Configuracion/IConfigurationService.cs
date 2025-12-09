using System.Threading.Tasks;
using WayPrecision.Domain.Models;

namespace WayPrecision.Domain.Services.Configuracion
{
    public interface IConfigurationService
    {
        Task<Configuration> GetOrCreateAsync();

        Task SaveAsync(Configuration updatedConfig);
    }
}