using SQLite;
using WayPrecision.Domain.Models;

namespace WayPrecision.Domain.Data.Repositories
{
    public class UnitsRepository : Repository<Unit>
    {
        public UnitsRepository(SQLiteAsyncConnection connection) : base(connection)
        {
        }
    }
}