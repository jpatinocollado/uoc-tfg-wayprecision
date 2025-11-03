using SQLite;
using WayPrecision.Domain.Models;

namespace WayPrecision.Domain.Data
{
    public class DatabaseContext
    {
        public SQLiteAsyncConnection Connection { get; }

        public DatabaseContext(string dbPath)
        {
            Connection = new SQLiteAsyncConnection(dbPath);
            Connection.CreateTableAsync<Configuration>().Wait();
            Connection.CreateTableAsync<Unit>().Wait();
            Connection.CreateTableAsync<Track>().Wait();
            Connection.CreateTableAsync<Position>().Wait();
            Connection.CreateTableAsync<TrackPoint>().Wait();
            Connection.CreateTableAsync<Waypoint>().Wait();
        }
    }
}