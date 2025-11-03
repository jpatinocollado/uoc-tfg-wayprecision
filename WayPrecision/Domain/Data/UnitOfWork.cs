using SQLite;
using WayPrecision.Domain.Data.Repositories;

namespace WayPrecision.Domain.Data
{
    public class UnitOfWork : IAsyncDisposable
    {
        private readonly SQLiteAsyncConnection _connection;
        private bool _inTransaction = false;

        public ConfigurationRepository Configurations { get; }
        public UnitsRepository Units { get; }
        public TracksRepository Tracks { get; }
        public PositionsRepository Positions { get; }
        public TrackPointRepository TrackPoints { get; }
        public WaypointsRepository Waypoints { get; }

        public UnitOfWork(DatabaseContext context)
        {
            _connection = context.Connection;

            Configurations = new ConfigurationRepository(_connection);
            Units = new UnitsRepository(_connection);
            Tracks = new TracksRepository(_connection);
            Positions = new PositionsRepository(_connection);
            TrackPoints = new TrackPointRepository(_connection);
            Waypoints = new WaypointsRepository(_connection);
        }

        public async Task BeginTransactionAsync()
        {
            if (_inTransaction) throw new InvalidOperationException("Ya existe una transacción activa.");
            await _connection.ExecuteAsync("BEGIN TRANSACTION;");
            _inTransaction = true;
        }

        public async Task CommitTransactionAsync()
        {
            if (!_inTransaction) throw new InvalidOperationException("No hay transacción activa.");
            await _connection.ExecuteAsync("COMMIT;");
            _inTransaction = false;
        }

        public async Task RollbackTransactionAsync()
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