using SQLite;
using WayPrecision.Domain.Models;

namespace WayPrecision.Domain.Data.Repositories
{
    public class ConfigurationRepository : Repository<Configuration>
    {
        public ConfigurationRepository(SQLiteAsyncConnection connection) : base(connection)
        {
        }
    }
}