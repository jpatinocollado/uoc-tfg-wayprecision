using SQLite;
using WayPrecision.Domain.Models;

namespace WayPrecision.Domain.Data.Repositories
{
    public class TracksRepository : Repository<Track>
    {
        public TracksRepository(SQLiteAsyncConnection connection) : base(connection)
        {
        }
    }
}