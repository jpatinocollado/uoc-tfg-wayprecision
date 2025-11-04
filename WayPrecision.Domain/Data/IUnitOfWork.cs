using WayPrecision.Domain.Data.Repositories;
using WayPrecision.Domain.Models;

namespace WayPrecision.Domain.Data
{
    /// <summary>
    /// Define la unidad de trabajo para coordinar operaciones sobre múltiples repositorios
    /// y gestionar la persistencia de los cambios en una única transacción.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Repositorio para la gestión de configuraciones.
        /// </summary>
        IRepository<Configuration> Configurations { get; }

        /// <summary>
        /// Repositorio para la gestión de unidades.
        /// </summary>
        IRepository<Unit> Units { get; }

        /// <summary>
        /// Repositorio para la gestión de tracks.
        /// </summary>
        IRepository<Track> Tracks { get; }

        /// <summary>
        /// Repositorio para la gestión de posiciones.
        /// </summary>
        IRepository<Position> Positions { get; }

        /// <summary>
        /// Repositorio para la gestión de puntos de track.
        /// </summary>
        IRepository<TrackPoint> TrackPoints { get; }

        /// <summary>
        /// Repositorio para la gestión de waypoints.
        /// </summary>
        IRepository<Waypoint> Waypoints { get; }

        /// <summary>
        /// Guarda todos los cambios pendientes en los repositorios en una única transacción.
        /// </summary>
        /// <returns>Total de filas afectadas por las operaciones realizadas.</returns>
        Task<int> SaveChangesAsync();
    }
}