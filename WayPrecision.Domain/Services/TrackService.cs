using System.Diagnostics;
using WayPrecision.Domain.Data.UnitOfWork;
using WayPrecision.Domain.Models;

namespace WayPrecision.Domain.Services
{
    /// <summary>
    /// Servicio para la gestión de entidades <see cref="Track"/>.
    /// Proporciona operaciones CRUD y utilidades relacionadas con tracks y sus posiciones asociadas.
    /// </summary>
    public class TrackService : IService<Track>
    {
        private readonly ConfigurationService _configurationService;
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="TrackService"/>.
        /// </summary>
        /// <param name="unitOfWork">Unidad de trabajo para coordinar operaciones sobre repositorios.</param>
        public TrackService(IUnitOfWork unitOfWork, ConfigurationService configurationService)
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
            // Implementación de demo: devuelve dos tracks de ejemplo.
            List<Track> Tracks = new List<Track>();

            string trackId = Guid.NewGuid().ToString();
            string position1 = Guid.NewGuid().ToString();
            string position2 = Guid.NewGuid().ToString();
            string position3 = Guid.NewGuid().ToString();

            Tracks.Add(new Track()
            {
                Guid = trackId,
                Name = "Demo Track",
                Observation = "This is a demo track.",
                Created = DateTime.UtcNow.AddMinutes(-35).ToString("o"),
                Finalized = DateTime.UtcNow.ToString("o"),
                IsOpened = false,
                TotalPoints = 3,
                TypeGeometry = TypeGeometry.Polygon,
                TrackPoints = new List<TrackPoint>() {
                    new TrackPoint(){
                         Guid = Guid.NewGuid().ToString(),
                         TrackGuid = trackId,
                         PositionGuid = position1,
                         Position = new Position(){
                            Guid = position1,
                            Latitude = 41.661003770342276,
                            Longitude = 0.5547822911068457,
                            Timestamp = DateTime.UtcNow.ToString("o"),
                            Accuracy = 20
                         }
                    },
                    new TrackPoint(){
                         Guid = Guid.NewGuid().ToString(),
                         TrackGuid = trackId,
                         PositionGuid = position2,
                         Position = new Position(){
                            Guid = position1,
                            Latitude = 41.6605906511649,
                            Longitude = 0.5552997755430279,
                            Timestamp = DateTime.UtcNow.AddSeconds(5).ToString("o"),
                            Accuracy = 20
                         }
                    },
                    new TrackPoint(){
                         Guid = Guid.NewGuid().ToString(),
                         TrackGuid = trackId,
                         PositionGuid = position3,
                         Position = new Position(){
                            Guid = position1,
                            Latitude = 41.661350708186475,
                            Longitude = 0.5555705834603986,
                            Timestamp = DateTime.UtcNow.AddSeconds(10).ToString("o"),
                            Accuracy = 20
                         }
                    }
                }
            });

            string t2 = Guid.NewGuid().ToString();
            string t2p1 = Guid.NewGuid().ToString();
            string t2p2 = Guid.NewGuid().ToString();
            string t2p3 = Guid.NewGuid().ToString();
            string t2p4 = Guid.NewGuid().ToString();

            Tracks.Add(new Track()
            {
                Guid = t2,
                Name = "Demo Track 2",
                Observation = "This is a 2 demo track.",
                Created = DateTime.UtcNow.AddMinutes(-35).ToString("o"),
                Finalized = DateTime.UtcNow.ToString("o"),
                IsOpened = false,
                TotalPoints = 3,
                TypeGeometry = TypeGeometry.Polygon,
                TrackPoints = new List<TrackPoint>() {
                    new TrackPoint(){
                         Guid = Guid.NewGuid().ToString(),
                         TrackGuid = t2,
                         PositionGuid = t2p1,
                         Position = new Position(){
                            Guid = t2p1,
                            Latitude = 41.66215253129646,
                            Longitude = 0.5533226915521051,
                            Timestamp = DateTime.UtcNow.ToString("o"),
                            Accuracy = 20
                         }
                    },
                    new TrackPoint(){
                         Guid = Guid.NewGuid().ToString(),
                         TrackGuid = t2,
                         PositionGuid = t2p2,
                         Position = new Position(){
                            Guid = t2p2,
                            Latitude = 41.662429274617224,
                            Longitude = 0.5539554704481465,
                            Timestamp = DateTime.UtcNow.AddSeconds(5).ToString("o"),
                            Accuracy = 20
                         }
                    },

                    new TrackPoint(){
                         Guid = Guid.NewGuid().ToString(),
                         TrackGuid = t2,
                         PositionGuid = t2p4,
                         Position = new Position(){
                            Guid = t2p4,
                            Latitude = 41.66162310597749,
                            Longitude = 0.5542021469669579,
                            Timestamp = DateTime.UtcNow.AddSeconds(15).ToString("o"),
                            Accuracy = 20
                         }
                    },
                    new TrackPoint(){
                         Guid = Guid.NewGuid().ToString(),
                         TrackGuid = t2,
                         PositionGuid = t2p3,
                         Position = new Position(){
                            Guid = t2p3,
                            Latitude = 41.661358391685724,
                            Longitude = 0.5535800931369118,
                            Timestamp = DateTime.UtcNow.AddSeconds(10).ToString("o"),
                            Accuracy = 20
                         }
                    }
                }
            });

            Configuration configuration = await _configurationService.GetOrCreateAsync();
            foreach (var track in Tracks)
                track.SetConfiguration(configuration);

            return Tracks;
        }

        /// <summary>
        /// Obtiene un track por su identificador único.
        /// </summary>
        /// <param name="guid">Identificador único del track.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona. El resultado contiene el track encontrado o <c>null</c> si no existe.
        /// </returns>
        public async Task<Track> GetByIdAsync(string guid)
        {
            Track Track = await _unitOfWork.Tracks.GetByIdAsync(guid);

            Configuration configuration = await _configurationService.GetOrCreateAsync();
            Track.SetConfiguration(configuration);
            
            //if (!string.IsNullOrWhiteSpace(Track.PositionGuid))
            //    Track.Position = await _unitOfWork.Positions.GetByIdAsync(Track.PositionGuid);

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
            Track entity = await GetByIdAsync(guid);

            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Eliminamos el track y su posición asociada

            _unitOfWork.Tracks.DeleteDeferred(entity);
            //_unitOfWork.Positions.DeleteDeferred(Track.Position);

            // Guardamos los cambios
            await _unitOfWork.SaveChangesAsync();

            return await Task.FromResult(true);
        }
    }
}