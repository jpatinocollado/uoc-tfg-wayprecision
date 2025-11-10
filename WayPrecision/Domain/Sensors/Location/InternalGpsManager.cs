using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WayPrecision.Domain.Sensors.Location
{
    public class InternalGpsManager : IGpsManager
    {
        public event EventHandler<LocationEventArgs>? PositionChanged;

        private CancellationTokenSource _cts;
        private bool _isListening;
        private TimeSpan GpsInterval;

        public InternalGpsManager()
        {
            _cts = new CancellationTokenSource();
            GpsInterval = new TimeSpan(0, 0, 2);
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
                    var location = await Geolocation.GetLastKnownLocationAsync() ?? await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best), _cts.Token);

                    if (location != null)
                    {
                        PositionChanged?.Invoke(this, new LocationEventArgs(new GpsLocation()
                        {
                            Guid = Guid.NewGuid(),
                            Latitude = location.Latitude,
                            Longitude = location.Longitude,
                            Altitude = location.Altitude,
                            Accuracy = location.Accuracy,
                            Course = location.Course,
                            Timestamp = location.Timestamp.UtcDateTime
                        }));
                    }

                    await Task.Delay((int)GpsInterval.TotalMilliseconds, _cts.Token);
                }
            }, _cts.Token);
        }

        public Task StopListeningAsync()
        {
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