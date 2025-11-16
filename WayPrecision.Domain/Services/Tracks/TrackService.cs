using System.Diagnostics;
using WayPrecision.Domain.Data.UnitOfWork;
using WayPrecision.Domain.Models;
using WayPrecision.Domain.Services.Configuracion;

namespace WayPrecision.Domain.Services.Tracks
{
    /// <summary>
    /// Servicio para la gestión de entidades <see cref="Track"/>.
    /// Proporciona operaciones CRUD y utilidades relacionadas con tracks y sus posiciones asociadas.
    /// </summary>
    public class TrackService : IService<Track>
    {
        private readonly IConfigurationService _configurationService;
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="TrackService"/>.
        /// </summary>
        /// <param name="unitOfWork">Unidad de trabajo para coordinar operaciones sobre repositorios.</param>
        public TrackService(IUnitOfWork unitOfWork, IConfigurationService configurationService)
        {
            _unitOfWork = unitOfWork;
            _configurationService = configurationService;
        }

        /// <summary>
        /// Obtiene todos los tracks almacenados.
        /// </summary>
        /// <returns>
        /// Una tarea que representa la operación asíncrona. El resultado contiene la lista de tracks.
        /// </returns>
        public async Task<List<Track>> GetAllAsync()
        {
            Configuration configuration = await _configurationService.GetOrCreateAsync();

            List<Track> Tracks = await _unitOfWork.Tracks.GetAllAsync();

            HashSet<string> tracksId = Tracks.Select(a => a.Guid).ToHashSet();
            List<TrackPoint> points = await _unitOfWork.TrackPoints.GetAllAsync(a => tracksId.Contains(a.TrackGuid));
            HashSet<string> positionsId = points.Select(a => a.PositionGuid).ToHashSet();
            List<Position> positions = await _unitOfWork.Positions.GetAllAsync(a => positionsId.Contains(a.Guid));

            foreach (var point in points)
                point.Position = positions.First(a => a.Guid == point.PositionGuid);

            foreach (var track in Tracks)
            {
                var trackPoints = points.Where(a => a.TrackGuid == track.Guid).ToList();

                track.TrackPoints.AddRange(trackPoints);
                track.SetConfiguration(configuration);
            }

            return Tracks.OrderByDescending(a => a.Created).ToList();
        }

        /// <summary>
        /// Obtiene un track por su identificador único.
        /// </summary>
        /// <param name="guid">Identificador único del track.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona. El resultado contiene el track encontrado o <c>null</c> si no existe.
        /// </returns>
        public async Task<Track?> GetByIdAsync(string guid)
        {
            Track? Track = await _unitOfWork.Tracks.GetByIdAsync(guid);

            if (Track == null)
                return null;

            Configuration configuration = await _configurationService.GetOrCreateAsync();

            List<TrackPoint> trackPoints = await _unitOfWork.TrackPoints.GetAllAsync(a => a.TrackGuid == Track.Guid);
            HashSet<string> positionsId = trackPoints.Select(a => a.PositionGuid).ToHashSet();
            List<Position> positions = await _unitOfWork.Positions.GetAllAsync(a => positionsId.Contains(a.Guid));

            foreach (var point in trackPoints)
                point.Position = positions.First(a => a.Guid == point.PositionGuid);

            Track.TrackPoints.AddRange(trackPoints);
            Track.SetConfiguration(configuration);

            return Track;
        }

        /// <summary>
        /// Añade un nuevo track y su posición asociada obligatoriamente.
        /// </summary>
        /// <param name="entity">Entidad <see cref="Track"/> a crear.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona. El resultado contiene el track creado.
        /// </returns>
        /// <exception cref="ArgumentNullException">Se lanza si <paramref name="entity"/> es <c>null</c>.</exception>
        public async Task<Track> CreateAsync(Track entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            DateTime utcNow = DateTime.UtcNow;

            if (string.IsNullOrEmpty(entity.Guid))
                entity.Guid = Guid.NewGuid().ToString();
            if (string.IsNullOrEmpty(entity.Created))
                entity.Created = utcNow.ToString("o");

            _unitOfWork.Tracks.AddDeferred(entity);

            foreach (TrackPoint trackPoint in entity.TrackPoints)
            {
                _unitOfWork.Positions.AddDeferred(trackPoint.Position);
                _unitOfWork.TrackPoints.AddDeferred(trackPoint);
            }

            await _unitOfWork.SaveChangesAsync();

            return await Task.FromResult(entity);
        }

        /// <summary>
        /// Edita un track existente.
        /// </summary>
        /// <param name="entity">Entidad <see cref="Track"/> a actualizar.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona. El resultado contiene el track actualizado.
        /// </returns>
        /// <exception cref="ArgumentNullException">Se lanza si <paramref name="entity"/> es <c>null</c>.</exception>
        public async Task<Track> UpdateAsync(Track entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _unitOfWork.Tracks.UpdateAsync(entity);

            return await Task.FromResult(entity);
        }

        /// <summary>
        /// Elimina un track existente.
        /// </summary>
        /// <param name="guid">Identificador único del track a eliminar.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona. El resultado indica si la eliminación fue exitosa.
        /// </returns>
        /// <exception cref="ArgumentNullException">Se lanza si el track no existe.</exception>
        public async Task<bool> DeleteAsync(string guid)
        {
            Track? entity = await GetByIdAsync(guid);

            if (entity == null)
                throw new NullReferenceException(nameof(entity));

            foreach (TrackPoint trackPoint in entity.TrackPoints)
            {
                _unitOfWork.TrackPoints.DeleteDeferred(trackPoint);
                _unitOfWork.Positions.DeleteDeferred(trackPoint.Position);
            }

            _unitOfWork.Tracks.DeleteDeferred(entity);

            // Guardamos los cambios
            await _unitOfWork.SaveChangesAsync();

            return await Task.FromResult(true);
        }
    }
}