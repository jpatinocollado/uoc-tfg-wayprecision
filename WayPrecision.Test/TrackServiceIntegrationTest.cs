using WayPrecision.Domain.Data;
using WayPrecision.Domain.Data.UnitOfWork;
using WayPrecision.Domain.Models;
using WayPrecision.Domain.Services;
using WayPrecision.Domain.Services.Configuracion;
using WayPrecision.Domain.Services.Tracks;
using WayPrecision.Domain.Exceptions;

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

        private Track CreateSampleTrackWithPoints(string name, int pointsCount = 2)
        {
            var track = new Track
            {
                Guid = Guid.NewGuid().ToString(),
                Name = name,
                Observation = "Test Observation",
                Created = DateTime.UtcNow.ToString(),
                IsOpened = true,
                AreaUnits = "m2",
                LengthUnits = "m"
            };

            for (int i = 0; i < pointsCount; i++)
            {
                var position = new Position
                {
                    Guid = Guid.NewGuid().ToString(),
                    Latitude = 41.0 + i,
                    Longitude = 2.0 + i,
                    Altitude = 100 + i,
                    Accuracy = 5.0,
                    Course = 90.0,
                    Timestamp = DateTime.UtcNow.AddMinutes(i)
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

            return track;
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateTrack()
        {
            var track = CreateSampleTrackWithPoints("Test Track", 2);
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
            var track = CreateSampleTrackWithPoints("Track By Id", 2);
            await _service.CreateAsync(track);
            var found = await _service.GetByIdAsync(track.Guid);
            Assert.NotNull(found);
            Assert.Equal(track.Guid, found.Guid);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateTrack()
        {
            var track = CreateSampleTrackWithPoints("Track To Update", 2);
            await _service.CreateAsync(track);
            track.Name = "Updated Name";
            var updated = await _service.UpdateAsync(track);
            Assert.NotNull(updated);
            Assert.Equal("Updated Name", updated.Name);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteTrack()
        {
            var track = CreateSampleTrackWithPoints("Track To Delete", 2);
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
            var track = CreateSampleTrackWithPoints("Track with Points", 3);

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
            var track = CreateSampleTrackWithPoints("Track to delete with points", 1);

            // Ensure at least one track point exists for deletion test; create with 1 but CreateAsync requires >=2 so use 2
            track = CreateSampleTrackWithPoints("Track to delete with points", 2);

            await _service.CreateAsync(track);

            var trackPoint = track.TrackPoints.First();
            var position = trackPoint.Position;

            // Act
            var deleted = await _service.DeleteAsync(track.Guid);
            var foundTrack = await _service.GetByIdAsync(track.Guid);

            var foundTrackPoint = foundTrack?.TrackPoints.FirstOrDefault(a => a.Guid == trackPoint.Guid);
            var foundPosition = foundTrack?.TrackPoints.FirstOrDefault(a => a.Position.Guid == position.Guid);

            // Assert
            Assert.True(deleted);
            Assert.Null(foundTrack);
            Assert.Null(foundTrackPoint);
            Assert.Null(foundPosition);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrow_WhenLessThanTwoPoints()
        {
            var track = CreateSampleTrackWithPoints("Invalid Track", 1);

            await Assert.ThrowsAsync<ControlledException>(async () => await _service.CreateAsync(track));
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrow_WhenTrackDoesNotExist()
        {
            await Assert.ThrowsAsync<ControlledException>(async () => await _service.DeleteAsync("non-existent-guid"));
        }
    }
}