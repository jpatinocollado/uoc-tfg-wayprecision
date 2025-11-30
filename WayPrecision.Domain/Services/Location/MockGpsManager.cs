using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WayPrecision.Domain.Models;

namespace WayPrecision.Domain.Services.Location
{
    public class MockGpsManager : IGpsManager
    {
        public event EventHandler<LocationEventArgs> PositionChanged;

        private CancellationTokenSource _cts;
        private bool _isListening;
        private TimeSpan GpsInterval;

        private int index = 0;
        private readonly List<LocationEventArgs> Locations = new List<LocationEventArgs>();

        public MockGpsManager()
        {
            _cts = new CancellationTokenSource();
            GpsInterval = new TimeSpan(0, 0, 30);

            InitPositions();
        }

        private void InitPositions()
        {
            Locations.Add(new LocationEventArgs(new Position()
            {
                Guid = Guid.NewGuid().ToString(),
                Latitude = 41.66215253129646,
                Longitude = 0.5533226915521051,
                Accuracy = 5,
                Course = 85.66,
            }));
            Locations.Add(new LocationEventArgs(new Position()
            {
                Guid = Guid.NewGuid().ToString(),
                Latitude = 41.662429274617224,
                Longitude = 0.5539554704481465,
                Accuracy = 4,
                Course = 120.12,
            }));
            Locations.Add(new LocationEventArgs(new Position()
            {
                Guid = Guid.NewGuid().ToString(),
                Latitude = 41.66162310597749,
                Longitude = 0.5542021469669579,
                Accuracy = 6,
                Course = 150.45,
            }));
            Locations.Add(new LocationEventArgs(new Position()
            {
                Guid = Guid.NewGuid().ToString(),
                Latitude = 41.661358391685724,
                Longitude = 0.5535800931369118,
                Accuracy = 20,
                Course = 200.78,
            }));
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
                    var location = Locations[index];
                    index++;

                    if (index >= Locations.Count)
                        index = 0;

                    location.Position.Timestamp = DateTime.UtcNow;

                    PositionChanged?.Invoke(this, location);

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