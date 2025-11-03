using System.Collections.Concurrent;
using SQLite;

namespace WayPrecision.Domain.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : class, new()
    {
        protected readonly SQLiteAsyncConnection _connection;

        // Usamos Func<Task<int>> porque SQLiteAsyncConnection.*Async devuelve Task<int>
        private readonly ConcurrentQueue<Func<Task<int>>> _pendingOperations = new();

        public Repository(SQLiteAsyncConnection connection)
        {
            _connection = connection;
        }

        public Task<List<T>> GetAllAsync() => _connection.Table<T>().ToListAsync();

        public Task<T> GetByIdAsync(string guid) => _connection.FindAsync<T>(guid);

        // Operaciones inmediatas
        public Task<int> InsertAsync(T entity) => _connection.InsertAsync(entity);

        public Task<int> UpdateAsync(T entity) => _connection.UpdateAsync(entity);

        public Task<int> DeleteAsync(T entity) => _connection.DeleteAsync(entity);

        // Operaciones diferidas (para SaveChanges)
        // Añaden funciones que devuelven Task<int>
        public void AddDeferred(T entity) => _pendingOperations.Enqueue(() => _connection.InsertAsync(entity));

        public void UpdateDeferred(T entity) => _pendingOperations.Enqueue(() => _connection.UpdateAsync(entity));

        public void DeleteDeferred(T entity) => _pendingOperations.Enqueue(() => _connection.DeleteAsync(entity));

        // Commit de todas las operaciones pendientes; devuelve total de filas afectadas
        internal async Task<int> CommitAsync()
        {
            int affected = 0;
            while (_pendingOperations.TryDequeue(out var operation))
            {
                // operation es Func<Task<int>>
                affected += await operation();
            }
            return affected;
        }
    }
}