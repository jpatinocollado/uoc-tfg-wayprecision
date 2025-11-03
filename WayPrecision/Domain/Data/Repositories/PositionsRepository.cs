using SQLite;
using WayPrecision.Domain.Models;

namespace WayPrecision.Domain.Data.Repositories
{
    public class PositionsRepository : Repository<Position>
    {
        public PositionsRepository(SQLiteAsyncConnection connection) : base(connection)
        {
        }
    }
}