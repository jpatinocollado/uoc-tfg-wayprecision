using WayPrecision.Domain.Models;
using System.Linq;
using WayPrecision.Domain.Data.UnitOfWork;

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
        public async Task<List<Waypoint>> GetAllAsync()
        {
            List<Waypoint> waypoints = await _unitOfWork.Waypoints.GetAllAsync();

            foreach (var waypoint in waypoints.Where(waypoint => !string.IsNullOrWhiteSpace(waypoint.PositionGuid)))
                waypoint.Position = await _unitOfWork.Positions.GetByIdAsync(waypoint.PositionGuid);

            return waypoints;
        }


        public async Task<Waypoint> GetByIdAsync(string guid)
        {
            Waypoint waypoint = await _unitOfWork.Waypoints.GetByIdAsync(guid);

            if( !string.IsNullOrWhiteSpace(waypoint.PositionGuid))
                waypoint.Position = await _unitOfWork.Positions.GetByIdAsync(waypoint.PositionGuid);

            return waypoint;
        }


        /// <summary>
        /// Añade un nuevo waypoint y su posición asociada obligatoriamente.
        /// </summary>
        public async Task AddAsync(Waypoint waypoint)
        {
            if (waypoint == null)
                throw new ArgumentNullException(nameof(waypoint));
            else if (waypoint.Position == null)
                throw new ArgumentNullException(nameof(waypoint.Position));

            DateTime utcNow = DateTime.UtcNow;

            if (string.IsNullOrEmpty(waypoint.Position.Guid))
                waypoint.Position.Guid = Guid.NewGuid().ToString();
            if (string.IsNullOrEmpty(waypoint.Position.Timestamp))
                waypoint.Position.Timestamp = utcNow.ToString("o");

            _unitOfWork.Positions.AddDeferred(waypoint.Position);

            if (string.IsNullOrEmpty(waypoint.Guid))
                waypoint.Guid = Guid.NewGuid().ToString();
            if (string.IsNullOrEmpty(waypoint.Created))
                waypoint.Created = utcNow.ToString("o");

            waypoint.PositionGuid = waypoint.Position.Guid;

            _unitOfWork.Waypoints.AddDeferred(waypoint);

            await _unitOfWork.SaveChangesAsync();
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

            //eliminamos el waypoint y su posición asociada
            _unitOfWork.Waypoints.DeleteDeferred(waypoint);
            _unitOfWork.Positions.DeleteDeferred(waypoint.Position);

            //guardamos los cambios
            await _unitOfWork.SaveChangesAsync();
        }
    }
}