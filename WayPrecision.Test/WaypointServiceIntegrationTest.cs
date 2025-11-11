using WayPrecision.Domain.Data;
using WayPrecision.Domain.Data.UnitOfWork;
using WayPrecision.Domain.Models;
using WayPrecision.Domain.Services;

namespace WayPrecision.Test
{
    public class WaypointServiceIntegrationTest
    {
        private readonly IService<Waypoint> _service;
        private readonly IUnitOfWork _unitOfWork;

        public WaypointServiceIntegrationTest()
        {
            var dbContext = new DatabaseContext("C:\\Users\\jpatinoc.INDRA\\AppData\\Local\\User Name\\com.companyname.wayprecision\\Data\\wayprecision.db3");
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
                Position = new Position
                {
                    Latitude = 41.660651600484606,
                    Longitude = 0.55507296355237346,
                    Accuracy = 5.0,
                    Altitude = null,
                    Course = null,
                }
            };

            // Act
            waypoint = await _service.CreateAsync(waypoint);
            var allWaypoints = await _service.GetAllAsync();
            var storedWaypoint = allWaypoints.FirstOrDefault(w => w.Name == "Test Waypoint");

            // Assert
            Assert.NotNull(storedWaypoint);
            Assert.False(string.IsNullOrEmpty(storedWaypoint.PositionGuid));

            Assert.NotNull(storedWaypoint.Position);
            Assert.Equal(waypoint.Position.Latitude, storedWaypoint.Position.Latitude);
            Assert.Equal(waypoint.Position.Longitude, storedWaypoint.Position.Longitude);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllWaypoints()
        {
            // Arrange
            var waypoint1 = new Waypoint
            {
                Name = "WP1",
                Observation = "O1",
                Position = new Position { Latitude = 1, Longitude = 2, }
            };
            var waypoint2 = new Waypoint
            {
                Name = "WP2",
                Observation = "O2",
                Position = new Position { Latitude = 3, Longitude = 4 }
            };
            waypoint1 = await _service.CreateAsync(waypoint1);
            waypoint2 = await _service.CreateAsync(waypoint2);

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
            var waypoint = new Waypoint
            {
                Name = "ToUpdate",
                Observation = "Old",
                Position = new Position { Latitude = 1, Longitude = 2 }
            };

            waypoint = await _service.CreateAsync(waypoint);
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
            var waypoint = new Waypoint
            {
                Name = "ToDelete",
                Observation = "Obs",
                Position = new Position { Latitude = 7, Longitude = 8 }
            };

            waypoint = await _service.CreateAsync(waypoint);
            var all = await _service.GetAllAsync();
            var stored = all.First(w => w.Name == "ToDelete");

            // Act
            await _service.DeleteAsync(waypoint.Guid);
            var afterDelete = await _service.GetAllAsync();

            // Assert
            Assert.DoesNotContain(afterDelete, w => w.Guid == stored.Guid);
        }
    }
}