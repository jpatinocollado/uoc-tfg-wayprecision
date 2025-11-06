using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WayPrecision.Domain.Data.UnitOfWork;
using WayPrecision.Domain.Models;

namespace WayPrecision.Domain.Services
{
    public class TrackService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TrackService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Obtiene todos los Tracks almacenados.
        /// </summary>
        public async Task<List<Track>> GetAllAsync()
        {
            List<Track> Tracks = await _unitOfWork.Tracks.GetAllAsync();

            //foreach (var Track in Tracks.Where(Track => !string.IsNullOrWhiteSpace(Track.PositionGuid)))
            //    Track.Position = await _unitOfWork.Positions.GetByIdAsync(Track.PositionGuid);

            return Tracks;
        }

        public async Task<Track> GetByIdAsync(string guid)
        {
            Track Track = await _unitOfWork.Tracks.GetByIdAsync(guid);

            //if (!string.IsNullOrWhiteSpace(Track.PositionGuid))
            //    Track.Position = await _unitOfWork.Positions.GetByIdAsync(Track.PositionGuid);

            return Track;
        }

        /// <summary>
        /// Añade un nuevo Track y su posición asociada obligatoriamente.
        /// </summary>
        public async Task AddAsync(Track Track)
        {
            if (Track == null)
                throw new ArgumentNullException(nameof(Track));
            //else if (Track.Position == null)
            //    throw new ArgumentNullException(nameof(Track.Position));

            DateTime utcNow = DateTime.UtcNow;

            //if (string.IsNullOrEmpty(Track.Position.Guid))
            //    Track.Position.Guid = Guid.NewGuid().ToString();
            //if (string.IsNullOrEmpty(Track.Position.Timestamp))
            //    Track.Position.Timestamp = utcNow.ToString("o");

            //_unitOfWork.Positions.AddDeferred(Track.Position);

            if (string.IsNullOrEmpty(Track.Guid))
                Track.Guid = Guid.NewGuid().ToString();
            if (string.IsNullOrEmpty(Track.Created))
                Track.Created = utcNow.ToString("o");

            //Track.PositionGuid = Track.Position.Guid;

            _unitOfWork.Tracks.AddDeferred(Track);

            await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Edita un Track existente.
        /// </summary>
        public async Task UpdateAsync(Track Track)
        {
            if (Track == null)
                throw new ArgumentNullException(nameof(Track));

            await _unitOfWork.Tracks.UpdateAsync(Track);
        }

        /// <summary>
        /// Elimina un Track existente.
        /// </summary>
        public async Task DeleteAsync(Track Track)
        {
            if (Track == null)
                throw new ArgumentNullException(nameof(Track));

            //eliminamos el Track y su posición asociada
            _unitOfWork.Tracks.DeleteDeferred(Track);
            //_unitOfWork.Positions.DeleteDeferred(Track.Position);

            //guardamos los cambios
            await _unitOfWork.SaveChangesAsync();
        }
    }
}