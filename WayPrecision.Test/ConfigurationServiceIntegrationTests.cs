using WayPrecision.Domain.Services;
using WayPrecision.Domain.Data;
using WayPrecision.Domain.Models;

namespace WayPrecision.Test
{
    public class ConfigurationServiceIntegrationTests
    {
        private readonly ConfigurationService _service;
        private readonly IUnitOfWork _unitOfWork;

        public ConfigurationServiceIntegrationTests()
        {
            var dbContext = new DatabaseContext(":memory:");
            _unitOfWork = new UnitOfWork(dbContext);
            _service = new ConfigurationService(_unitOfWork);
        }

        [Fact]
        public async Task GetOrCreateAsync_ShouldCreateDefaultRecord_WhenNoneExists()
        {
            // Act
            var config = await _service.GetOrCreateAsync();

            // Assert
            Assert.NotNull(config);
            Assert.Equal(UnitEnum.MetrosCuadrados.ToString(), config.AreaUnits);
            Assert.Equal(UnitEnum.Metros.ToString(), config.LengthUnits);
        }

        [Fact]
        public async Task GetOrCreateAsync_ShouldReturnSameRecord_WhenAlreadyExists()
        {
            // Arrange
            var first = await _service.GetOrCreateAsync();

            // Act
            var second = await _service.GetOrCreateAsync();

            // Assert
            Assert.Equal(first.Guid, second.Guid); // mismo registro
        }

        [Fact]
        public async Task SaveAsync_ShouldUpdateRecord()
        {
            // Arrange
            var config = await _service.GetOrCreateAsync();
            config.LengthUnits = UnitEnum.Kilometros.ToString();
            config.AreaUnits = UnitEnum.KilometrosCuadrados.ToString();

            // Act
            await _service.SaveAsync(config);

            // Assert
            var updated = await _service.GetOrCreateAsync();

            Assert.NotNull(config);
            Assert.Equal(UnitEnum.KilometrosCuadrados.ToString(), updated.AreaUnits);
            Assert.Equal(UnitEnum.Kilometros.ToString(), updated.LengthUnits);
        }
    }
}