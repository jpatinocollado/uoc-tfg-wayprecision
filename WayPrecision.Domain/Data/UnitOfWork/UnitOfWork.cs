using SQLite;
using WayPrecision.Domain.Data.Repositories;
using WayPrecision.Domain.Models;

namespace WayPrecision.Domain.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork, IAsyncDisposable
    {
        private readonly SQLiteAsyncConnection _connection;
        private bool _inTransaction = false;

        public IRepository<Configuration> Configurations { get; }
        public IRepository<Unit> Units { get; }
        public IRepository<Track> Tracks { get; }
        public IRepository<Position> Positions { get; }
        public IRepository<TrackPoint> TrackPoints { get; }
        public IRepository<Waypoint> Waypoints { get; }

        public UnitOfWork(DatabaseContext context)
        {
            _connection = context.Connection;

            Configurations = new Repository<Configuration>(_connection);
            Units = new Repository<Unit>(_connection);
            Tracks = new Repository<Track>(_connection);
            Positions = new Repository<Position>(_connection);
            TrackPoints = new Repository<TrackPoint>(_connection);
            Waypoints = new Repository<Waypoint>(_connection);
        }

        private async Task BeginTransactionAsync()
        {
            if (_inTransaction)
                throw new InvalidOperationException("Ya existe una transacción activa.");

            await _connection.ExecuteAsync("BEGIN TRANSACTION;");
            _inTransaction = true;
        }

        private async Task CommitTransactionAsync()
        {
            if (!_inTransaction) throw new InvalidOperationException("No hay transacción activa.");
            await _connection.ExecuteAsync("COMMIT;");
            _inTransaction = false;
        }

        private async Task RollbackTransactionAsync()
        {
            if (_inTransaction)
            {
                await _connection.ExecuteAsync("ROLLBACK;");
                _inTransaction = false;
            }
        }

        /// <summary>
        /// Guarda todos los cambios pendientes en una transacción única.
        /// </summary>
        public async Task<int> SaveChangesAsync()
        {
            int totalAffected = 0;

            await BeginTransactionAsync();
            try
            {
                totalAffected += await Configurations.CommitAsync();
                totalAffected += await Units.CommitAsync();
                totalAffected += await Tracks.CommitAsync();
                totalAffected += await Positions.CommitAsync();
                totalAffected += await TrackPoints.CommitAsync();
                totalAffected += await Waypoints.CommitAsync();

                await CommitTransactionAsync();
                return totalAffected;
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_inTransaction)
                await RollbackTransactionAsync();
        }
    }
}