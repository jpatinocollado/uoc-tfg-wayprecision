using WayPrecision.Domain.Data;
using WayPrecision.Domain.Models;

namespace WayPrecision.Domain.Services
{
    /// <summary>
    /// Servicio encargado de gestionar los waypoints.
    /// </summary>
    public class WaypointService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WaypointService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Obtiene todos los waypoints almacenados.
        /// </summary>
        public Task<List<Waypoint>> GetAllAsync()
        {
            return _unitOfWork.Waypoints.GetAllAsync();
        }

        /// <summary>
        /// Añade un nuevo waypoint y su posición asociada obligatoriamente.
        /// </summary>
        public async Task AddAsync(Waypoint waypoint, Position position)
        {
            if (waypoint == null)
                throw new ArgumentNullException(nameof(waypoint));
            if (position == null)
                throw new ArgumentNullException(nameof(position));

            if (string.IsNullOrEmpty(position.Guid))
                position.Guid = Guid.NewGuid().ToString();

            // Guardar la posición primero
            await _unitOfWork.Positions.InsertAsync(position);

            if (string.IsNullOrEmpty(waypoint.Guid))
                waypoint.Guid = Guid.NewGuid().ToString();
            if (string.IsNullOrEmpty(waypoint.Created))
                waypoint.Created = DateTime.UtcNow.ToString("o");
            // Asociar el Guid de la posición
            waypoint.Position = position.Guid;

            await _unitOfWork.Waypoints.InsertAsync(waypoint);
        }

        /// <summary>
        /// Edita un waypoint existente.
        /// </summary>
        public async Task UpdateAsync(Waypoint waypoint)
        {
            if (waypoint == null)
                throw new ArgumentNullException(nameof(waypoint));
            await _unitOfWork.Waypoints.UpdateAsync(waypoint);
        }

        /// <summary>
        /// Elimina un waypoint existente.
        /// </summary>
        public async Task DeleteAsync(Waypoint waypoint)
        {
            if (waypoint == null)
                throw new ArgumentNullException(nameof(waypoint));
            await _unitOfWork.Waypoints.DeleteAsync(waypoint);
        }
    }
}