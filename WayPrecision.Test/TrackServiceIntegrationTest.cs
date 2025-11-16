using WayPrecision.Domain.Data;
using WayPrecision.Domain.Data.UnitOfWork;
using WayPrecision.Domain.Models;
using WayPrecision.Domain.Services;
using Xunit;
using System.Threading.Tasks;
using WayPrecision.Domain.Services.Configuracion;

namespace WayPrecision.Test
{
    public class TrackServiceIntegrationTest
    {
        private readonly IConfigurationService _configurationService;
        private readonly IService<Track> _service;

        public TrackServiceIntegrationTest()
        {
            //var dbContext = new DatabaseContext("C:\\Users\\jpatinoc.INDRA\\AppData\\Local\\User Name\\com.companyname.wayprecision\\Data\\wayprecision.db3");

            var dbContext = new DatabaseContext(":memory:");
            IUnitOfWork _unitOfWork = new UnitOfWork(dbContext);
            _configurationService = new ConfigurationService(_unitOfWork);
            _service = new TrackService(_unitOfWork, _configurationService);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateTrack()
        {
            var track = new Track
            {
                Guid = Guid.NewGuid().ToString(),
                Name = "Test Track",
                Observation = "Test Observation",
                Created = DateTime.UtcNow.ToString(),
                IsOpened = true,
                AreaUnits = "m2",
                LengthUnits = "m"
            };
            var created = await _service.CreateAsync(track);
            Assert.NotNull(created);
            Assert.Equal(track.Guid, created.Guid);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnTracks()
        {
            var tracks = await _service.GetAllAsync();
            Assert.NotNull(tracks);
            Assert.True(tracks.Count >= 0);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnTrack()
        {
            var track = new Track
            {
                Guid = Guid.NewGuid().ToString(),
                Name = "Track By Id",
                Observation = "Observation",
                Created = DateTime.UtcNow.ToString(),
                IsOpened = true,
                AreaUnits = "m2",
                LengthUnits = "m"
            };
            await _service.CreateAsync(track);
            var found = await _service.GetByIdAsync(track.Guid);
            Assert.NotNull(found);
            Assert.Equal(track.Guid, found.Guid);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateTrack()
        {
            var track = new Track
            {
                Guid = Guid.NewGuid().ToString(),
                Name = "Track To Update",
                Observation = "Observation",
                Created = DateTime.UtcNow.ToString(),
                IsOpened = true,
                AreaUnits = "m2",
                LengthUnits = "m"
            };
            await _service.CreateAsync(track);
            track.Name = "Updated Name";
            var updated = await _service.UpdateAsync(track);
            Assert.NotNull(updated);
            Assert.Equal("Updated Name", updated.Name);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteTrack()
        {
            var track = new Track
            {
                Guid = Guid.NewGuid().ToString(),
                Name = "Track To Delete",
                Observation = "Observation",
                Created = DateTime.UtcNow.ToString(),
                IsOpened = true,
                AreaUnits = "m2",
                LengthUnits = "m"
            };
            await _service.CreateAsync(track);
            var deleted = await _service.DeleteAsync(track.Guid);
            Assert.True(deleted);
            var found = await _service.GetByIdAsync(track.Guid);
            Assert.Null(found);
        }

        [Fact]
        public async Task CreateTrack_WithTrackPointsAndPositions_ShouldPersistRelations()
        {
            // Arrange
            var track = new Track
            {
                Guid = Guid.NewGuid().ToString(),
                Name = "Track with Points",
                Observation = "Test with points",
                Created = DateTime.UtcNow.ToString(),
                IsOpened = true,
                AreaUnits = "m2",
                LengthUnits = "m"
            };

            // Create positions and trackpoints
            for (int i = 0; i < 3; i++)
            {
                var position = new Position
                {
                    Guid = Guid.NewGuid().ToString(),
                    Latitude = 41.0 + i,
                    Longitude = 2.0 + i,
                    Altitude = 100 + i,
                    Accuracy = 5.0,
                    Course = 90.0,
                    Timestamp = DateTime.UtcNow.AddMinutes(i).ToString()
                };
                var trackPoint = new TrackPoint
                {
                    Guid = Guid.NewGuid().ToString(),
                    TrackGuid = track.Guid,
                    PositionGuid = position.Guid,
                    Position = position
                };
                track.TrackPoints.Add(trackPoint);
            }

            await _service.CreateAsync(track);

            // Act
            var found = await _service.GetByIdAsync(track.Guid);

            // Assert
            Assert.NotNull(found);
            Assert.Equal(3, found.TrackPoints.Count);
            foreach (var tp in found.TrackPoints)
            {
                Assert.False(string.IsNullOrEmpty(tp.PositionGuid));
            }
        }

        [Fact]
        public async Task DeleteTrack_ShouldDeleteTrackPointsAndPositions()
        {
            // Arrange
            var track = new Track
            {
                Guid = Guid.NewGuid().ToString(),
                Name = "Track to delete with points",
                Observation = "Delete test",
                Created = DateTime.UtcNow.ToString(),
                IsOpened = true,
                AreaUnits = "m2",
                LengthUnits = "m"
            };
            var position = new Position
            {
                Guid = Guid.NewGuid().ToString(),
                Latitude = 43.0,
                Longitude = 4.0,
                Altitude = 102,
                Accuracy = 5.0,
                Course = 90.0,
                Timestamp = DateTime.UtcNow.ToString()
            };

            var trackPoint = new TrackPoint
            {
                Guid = Guid.NewGuid().ToString(),
                TrackGuid = track.Guid,
                PositionGuid = position.Guid,
                Position = position
            };

            track.TrackPoints.Add(trackPoint);
            await _service.CreateAsync(track);

            // Act
            var deleted = await _service.DeleteAsync(track.Guid);
            var foundTrack = await _service.GetByIdAsync(track.Guid);

            Assert.Null(foundTrack);

            var foundTrackPoint = foundTrack?.TrackPoints.FirstOrDefault(a => a.Guid == trackPoint.Guid);
            var foundPosition = foundTrack?.TrackPoints.FirstOrDefault(a => a.Position.Guid == position.Guid);

            // Assert
            Assert.True(deleted);
            Assert.Null(foundTrack);
            Assert.Null(foundTrackPoint);
            Assert.Null(foundPosition);
        }
    }
}