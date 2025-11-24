using WayPrecision.Domain.Models;
using WayPrecision.Domain.Services.Configuracion;

namespace WayPrecision.Domain.Services.Location
{
    public class MockCsvGpsManager : IGpsManager
    {
        public event EventHandler<LocationEventArgs>? PositionChanged;

        private readonly IConfigurationService _configurationService;

        private CancellationTokenSource _cts;
        private bool _isListening;
        private TimeSpan GpsInterval;

        private int index = 0;
        private readonly List<LocationEventArgs> Locations = [];

        public MockCsvGpsManager(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
            _cts = new CancellationTokenSource();
            GpsInterval = new TimeSpan(0, 0, 2);

            _ = InitMock();
        }

        private async Task InitMock()
        {
            var configuration = await _configurationService.GetOrCreateAsync();
            var csvLines = configuration.CsvPositions?.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

            if (csvLines is null)
                return;

            foreach (var line in csvLines)
            {
                var parts = line.Split(',');
                if (double.TryParse(parts[0].Replace('.', ','), out double latitude) &&
                    double.TryParse(parts[1].Replace('.', ','), out double longitude))
                {
                    Locations.Add(new LocationEventArgs(new Position()
                    {
                        Guid = Guid.NewGuid().ToString(),
                        Latitude = latitude,
                        Longitude = longitude,
                    }));
                }
            }
        }

        public async Task StartListeningAsync(TimeSpan gpsInterval)
        {
            GpsInterval = gpsInterval;

            if (_cts is null || _cts.IsCancellationRequested)
                _cts = new CancellationTokenSource();

            if (_isListening)
                return;

            _isListening = true;

            await Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    Task.Delay((int) new TimeSpan(0 ,0 ,5).TotalMilliseconds);

                    if (index < Locations.Count)
                    {
                        var location = Locations[index];
                        index++;

                        location.Position.Timestamp = DateTime.UtcNow;
                        PositionChanged?.Invoke(this, location);
                    }
                    await Task.Delay((int)GpsInterval.TotalMilliseconds, _cts.Token);
                }
            }, _cts.Token);
        }

        public Task StopListeningAsync()
        {
            index = 0;

            if (!_isListening)
                return Task.CompletedTask;

            _cts?.Cancel();
            _cts?.Dispose();
            _isListening = false;
            return Task.CompletedTask;
        }

        public Task ChangeGpsInterval(TimeSpan gpsInterval)
        {
            GpsInterval = gpsInterval;
            return Task.CompletedTask;
        }
    }
}