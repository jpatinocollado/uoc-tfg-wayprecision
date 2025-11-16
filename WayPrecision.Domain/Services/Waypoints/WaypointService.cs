using WayPrecision.Domain.Models;
using System.Linq;
using WayPrecision.Domain.Data.UnitOfWork;

namespace WayPrecision.Domain.Services.Waypoints
{
    /// <summary>
    /// Servicio encargado de gestionar los waypoints.
    /// </summary>
    public class WaypointService : IService<Waypoint>
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

            return waypoints.OrderByDescending(a => a.Created).ToList();
        }

        public async Task<Waypoint?> GetByIdAsync(string guid)
        {
            Waypoint waypoint = await _unitOfWork.Waypoints.GetByIdAsync(guid);

            if (waypoint == null)
                return null;

            if (!string.IsNullOrWhiteSpace(waypoint.PositionGuid))
                waypoint.Position = await _unitOfWork.Positions.GetByIdAsync(waypoint.PositionGuid);

            return waypoint;
        }

        /// <summary>
        /// Añade un nuevo waypoint y su posición asociada obligatoriamente.
        /// </summary>
        public async Task<Waypoint?> CreateAsync(Waypoint waypoint)

        {
            if (waypoint == null || waypoint.Position == null)
                return null;

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

            return await Task.FromResult(waypoint);
        }

        /// <summary>
        /// Edita un waypoint existente.
        /// </summary>
        public async Task<Waypoint?> UpdateAsync(Waypoint waypoint)
        {
            if (waypoint == null)
                return null;

            // Verifica si existe el waypoint en la base de datos
            var existing = await GetByIdAsync(waypoint.Guid);
            if (existing == null)
                return null;

            await _unitOfWork.Waypoints.UpdateAsync(waypoint);
            await _unitOfWork.SaveChangesAsync();

            return waypoint;
        }

        /// <summary>
        /// Elimina un waypoint existente.
        /// </summary>
        public async Task<bool> DeleteAsync(string guid)
        {
            Waypoint? waypoint = await GetByIdAsync(guid);
            if (waypoint == null)
                return await Task.FromResult(false);

            _unitOfWork.Waypoints.DeleteDeferred(waypoint);

            //eliminamos el waypoint y su posición asociada
            if (waypoint.Position != null)
                _unitOfWork.Positions.DeleteDeferred(waypoint.Position);

            //guardamos los cambios
            await _unitOfWork.SaveChangesAsync();

            return await Task.FromResult(true);
        }
    }
}