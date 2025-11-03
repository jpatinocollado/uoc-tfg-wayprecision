using SQLite;
using WayPrecision.Domain.Models;

namespace WayPrecision.Domain.Data.Repositories
{
    public class TrackPointRepository : Repository<TrackPoint>
    {
        public TrackPointRepository(SQLiteAsyncConnection connection) : base(connection)
        {
        }
    }
}