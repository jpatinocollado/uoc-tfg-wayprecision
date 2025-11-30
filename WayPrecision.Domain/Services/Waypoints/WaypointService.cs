using WayPrecision.Domain.Models;
using System.Linq;
using WayPrecision.Domain.Data.UnitOfWork;
using WayPrecision.Domain.Exceptions;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

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
        public async Task<Waypoint?> CreateAsync(Waypoint? waypoint)

        {
            if (waypoint == null || waypoint.Position == null)
                throw new ControlledException("El Waypoint y/o su posición no pueden ser nulos.");

            // Comprueba si ya existe un waypoint con el mismo nombre (ignora mayúsculas/minúsculas y espacios)
            string? newName = waypoint.Name?.Trim();

            if (string.IsNullOrEmpty(newName))
                throw new ControlledException("El nombre del Waypoint no puede estar vacío.");

            var allWaypoints = await _unitOfWork.Waypoints.GetAllAsync();
            if (allWaypoints.Any(w => string.Equals(w.Name.Trim(), newName, StringComparison.OrdinalIgnoreCase)))
                throw new ControlledException($"Ya existe un waypoint con el nombre '{newName}'.");

            DateTime utcNow = DateTime.UtcNow;

            if (string.IsNullOrEmpty(waypoint.Position.Guid))
                waypoint.Position.Guid = Guid.NewGuid().ToString();

            //if (string.IsNullOrEmpty(waypoint.Position.Timestamp))
            //    waypoint.Position.Timestamp = utcNow.ToString("o");

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
        public async Task<Waypoint?> UpdateAsync(Waypoint? waypoint)
        {
            if (waypoint == null)
                throw new ControlledException("El Waypoint no puede ser nulo.");

            // Verifica si existe el waypoint en la base de datos
            var existing = await GetByIdAsync(waypoint.Guid);
            if (existing == null)
                throw new ControlledException("El Waypoint que se intenta actualizar no existe.");

            // Comprueba si ya existe un waypoint con el mismo nombre (ignora mayúsculas/minúsculas y espacios)
            string? newName = waypoint.Name?.Trim();
            if (string.IsNullOrEmpty(newName))
                throw new ControlledException("El nombre del Waypoint no puede estar vacío.");

            var allWaypoints = await _unitOfWork.Waypoints.GetAllAsync();
            if (allWaypoints.Any(w => string.Equals(w.Name.Trim(), newName, StringComparison.OrdinalIgnoreCase)
                                                                           && w.Guid != waypoint.Guid))
                throw new ControlledException($"Ya existe un waypoint con el nombre '{newName}'.");

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
                throw new ControlledException("El Waypoint que se intenta eliminar no existe.");

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