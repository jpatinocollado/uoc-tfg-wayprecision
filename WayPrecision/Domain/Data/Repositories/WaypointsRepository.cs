using SQLite;
using WayPrecision.Domain.Models;

namespace WayPrecision.Domain.Data.Repositories
{
    public class WaypointsRepository : Repository<Waypoint>
    {
        public WaypointsRepository(SQLiteAsyncConnection connection) : base(connection)
        {
        }
    }
}