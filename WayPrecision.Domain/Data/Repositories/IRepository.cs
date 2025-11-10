using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WayPrecision.Domain.Data.Repositories
{
    /// <summary>
    /// Define las operaciones CRUD básicas y operaciones diferidas para un repositorio de entidades de tipo <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad gestionada por el repositorio.</typeparam>
    public interface IRepository<T> where T : class, new()
    {
        /// <summary>
        /// Obtiene todas las entidades de la tabla correspondiente.
        /// </summary>
        /// <returns>Una lista de todas las entidades.</returns>
        Task<List<T>> GetAllAsync();

        /// <summary>
        /// Obtiene una entidad por su identificador único.
        /// </summary>
        /// <param name="guid">Identificador único de la entidad.</param>
        /// <returns>La entidad encontrada o <c>null</c> si no existe.</returns>
        Task<T> GetByIdAsync(string guid);

        /// <summary>
        /// Inserta una nueva entidad en la base de datos de forma inmediata.
        /// </summary>
        /// <param name="entity">Entidad a insertar.</param>
        /// <returns>Número de filas afectadas.</returns>
        Task<int> InsertAsync(T entity);

        /// <summary>
        /// Actualiza una entidad existente en la base de datos de forma inmediata.
        /// </summary>
        /// <param name="entity">Entidad a actualizar.</param>
        /// <returns>Número de filas afectadas.</returns>
        Task<int> UpdateAsync(T entity);

        /// <summary>
        /// Elimina una entidad existente de la base de datos de forma inmediata.
        /// </summary>
        /// <param name="entity">Entidad a eliminar.</param>
        /// <returns>Número de filas afectadas.</returns>
        Task<int> DeleteAsync(T entity);

        /// <summary>
        /// Añade una operación de inserción diferida a la cola de operaciones pendientes.
        /// </summary>
        /// <param name="entity">Entidad a insertar.</param>
        void AddDeferred(T entity);

        /// <summary>
        /// Añade una operación de actualización diferida a la cola de operaciones pendientes.
        /// </summary>
        /// <param name="entity">Entidad a actualizar.</param>
        void UpdateDeferred(T entity);

        /// <summary>
        /// Añade una operación de eliminación diferida a la cola de operaciones pendientes.
        /// </summary>
        /// <param name="entity">Entidad a eliminar.</param>
        void DeleteDeferred(T entity);

        /// <summary>
        /// Ejecuta todas las operaciones diferidas pendientes y devuelve el total de filas afectadas.
        /// </summary>
        /// <returns>Total de filas afectadas por las operaciones ejecutadas.</returns>
        Task<int> CommitAsync();
    }
}