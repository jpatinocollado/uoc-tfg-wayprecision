using System.Collections.Concurrent;
using System.Linq.Expressions;
using SQLite;

namespace WayPrecision.Domain.Data.Repositories
{
    /// <summary>
    /// Repositorio genérico para operaciones CRUD y soporte de operaciones diferidas sobre una tabla SQLite.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad gestionada por el repositorio.</typeparam>
    public class Repository<T> : IRepository<T> where T : class, new()
    {
        /// <summary>
        /// Conexión asíncrona a la base de datos SQLite.
        /// </summary>
        protected readonly SQLiteAsyncConnection _connection;

        /// <summary>
        /// Cola de operaciones diferidas pendientes de ejecutar.
        /// </summary>
        private readonly ConcurrentQueue<Func<Task<int>>> _pendingOperations = new();

        /// <summary>
        /// Inicializa una nueva instancia del repositorio con la conexión especificada.
        /// </summary>
        /// <param name="connection">Conexión SQLite asíncrona.</param>
        public Repository(SQLiteAsyncConnection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// Obtiene todas las entidades de la tabla.
        /// </summary>
        /// <returns>Lista de todas las entidades.</returns>
        public virtual Task<List<T>> GetAllAsync() => _connection.Table<T>().ToListAsync();

        /// <summary>
        /// Obtiene todas las entidades de la tabla aplicando el predicado.
        /// </summary>
        /// <returns>Lista de todas las entidades.</returns>
        public virtual Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate) => _connection.Table<T>().Where(predicate).ToListAsync();

        /// <summary>
        /// Obtiene una entidad por su identificador.
        /// </summary>
        /// <param name="guid">Identificador único de la entidad.</param>
        /// <returns>Entidad encontrada o null si no existe.</returns>
        public Task<T> GetByIdAsync(string guid) => _connection.FindAsync<T>(guid);

        /// <summary>
        /// Inserta una entidad de forma inmediata en la base de datos.
        /// </summary>
        /// <param name="entity">Entidad a insertar.</param>
        /// <returns>Número de filas afectadas.</returns>
        public Task<int> InsertAsync(T entity) => _connection.InsertAsync(entity);

        /// <summary>
        /// Actualiza una entidad de forma inmediata en la base de datos.
        /// </summary>
        /// <param name="entity">Entidad a actualizar.</param>
        /// <returns>Número de filas afectadas.</returns>
        public Task<int> UpdateAsync(T entity) => _connection.UpdateAsync(entity);

        /// <summary>
        /// Elimina una entidad de forma inmediata en la base de datos.
        /// </summary>
        /// <param name="entity">Entidad a eliminar.</param>
        /// <returns>Número de filas afectadas.</returns>
        public Task<int> DeleteAsync(T entity) => _connection.DeleteAsync(entity);

        /// <summary>
        /// Añade una operación de inserción diferida a la cola de operaciones pendientes.
        /// </summary>
        /// <param name="entity">Entidad a insertar.</param>
        public void AddDeferred(T entity) => _pendingOperations.Enqueue(() => _connection.InsertAsync(entity));

        /// <summary>
        /// Añade una operación de actualización diferida a la cola de operaciones pendientes.
        /// </summary>
        /// <param name="entity">Entidad a actualizar.</param>
        public void UpdateDeferred(T entity) => _pendingOperations.Enqueue(() => _connection.UpdateAsync(entity));

        /// <summary>
        /// Añade una operación de eliminación diferida a la cola de operaciones pendientes.
        /// </summary>
        /// <param name="entity">Entidad a eliminar.</param>
        public void DeleteDeferred(T entity) => _pendingOperations.Enqueue(() => _connection.DeleteAsync(entity));

        /// <summary>
        /// Ejecuta todas las operaciones diferidas pendientes y devuelve el total de filas afectadas.
        /// </summary>
        /// <returns>Total de filas afectadas por las operaciones ejecutadas.</returns>
        public async Task<int> CommitAsync()
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