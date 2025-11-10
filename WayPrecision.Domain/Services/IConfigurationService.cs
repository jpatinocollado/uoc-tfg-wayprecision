using WayPrecision.Domain.Models;

namespace WayPrecision.Domain.Services
{
    public interface IConfigurationService
    {
        Task<Configuration> GetOrCreateAsync();

        Task SaveAsync(Configuration updatedConfig);
    }
}