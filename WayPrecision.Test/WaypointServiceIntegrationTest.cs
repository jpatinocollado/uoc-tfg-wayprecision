using WayPrecision.Domain.Data;
using WayPrecision.Domain.Models;
using WayPrecision.Domain.Services;

namespace WayPrecision.Test
{
    public class WaypointServiceIntegrationTest
    {
        private readonly WaypointService _service;
        private readonly IUnitOfWork _unitOfWork;

        public WaypointServiceIntegrationTest()
        {
            var dbContext = new DatabaseContext(":memory:");
            _unitOfWork = new UnitOfWork(dbContext);
            _service = new WaypointService(_unitOfWork);
        }

        [Fact]
        public async Task AddAsync_ShouldAddWaypointAndPosition()
        {
            // Arrange
            var waypoint = new Waypoint
            {
                Name = "Test Waypoint",
                Observation = "Obs 1"
            };
            var position = new Position
            {
                Latitude = 10.1,
                Longitude = 20.2,
                Altitude = 100,
                Accuracy = 5,
                Course = 90,
                Timestamp = DateTime.UtcNow.ToString("o")
            };

            // Act
            await _service.AddAsync(waypoint, position);
            var allWaypoints = await _service.GetAllAsync();
            var storedWaypoint = allWaypoints.FirstOrDefault(w => w.Name == "Test Waypoint");

            // Assert
            Assert.NotNull(storedWaypoint);
            Assert.False(string.IsNullOrEmpty(storedWaypoint.Position));
            var storedPosition = await _unitOfWork.Positions.GetByIdAsync(storedWaypoint.Position);
            Assert.NotNull(storedPosition);
            Assert.Equal(position.Latitude, storedPosition.Latitude);
            Assert.Equal(position.Longitude, storedPosition.Longitude);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllWaypoints()
        {
            // Arrange
            var waypoint1 = new Waypoint { Name = "WP1", Observation = "O1" };
            var position1 = new Position { Latitude = 1, Longitude = 2, Timestamp = DateTime.UtcNow.ToString("o") };
            var waypoint2 = new Waypoint { Name = "WP2", Observation = "O2" };
            var position2 = new Position { Latitude = 3, Longitude = 4, Timestamp = DateTime.UtcNow.ToString("o") };
            await _service.AddAsync(waypoint1, position1);
            await _service.AddAsync(waypoint2, position2);

            // Act
            var all = await _service.GetAllAsync();

            // Assert
            Assert.True(all.Count >= 2);
            Assert.Contains(all, w => w.Name == "WP1");
            Assert.Contains(all, w => w.Name == "WP2");
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateWaypoint()
        {
            // Arrange
            var waypoint = new Waypoint { Name = "ToUpdate", Observation = "Old" };
            var position = new Position { Latitude = 5, Longitude = 6, Timestamp = DateTime.UtcNow.ToString("o") };
            await _service.AddAsync(waypoint, position);
            var all = await _service.GetAllAsync();
            var stored = all.First(w => w.Name == "ToUpdate");
            stored.Observation = "Updated";

            // Act
            await _service.UpdateAsync(stored);
            var updated = (await _service.GetAllAsync()).First(w => w.Guid == stored.Guid);

            // Assert
            Assert.Equal("Updated", updated.Observation);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveWaypoint()
        {
            // Arrange
            var waypoint = new Waypoint { Name = "ToDelete", Observation = "Obs" };
            var position = new Position { Latitude = 7, Longitude = 8, Timestamp = DateTime.UtcNow.ToString("o") };
            await _service.AddAsync(waypoint, position);
            var all = await _service.GetAllAsync();
            var stored = all.First(w => w.Name == "ToDelete");

            // Act
            await _service.DeleteAsync(stored);
            var afterDelete = await _service.GetAllAsync();

            // Assert
            Assert.DoesNotContain(afterDelete, w => w.Guid == stored.Guid);
        }
    }
}