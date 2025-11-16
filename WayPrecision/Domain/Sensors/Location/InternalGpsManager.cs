using WayPrecision.Domain.Models;
using WayPrecision.Domain.Services.Location;
using Microsoft.Maui.ApplicationModel; // For MainThread and Permissions

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
                    try
                    {
                        // Comprueba y solicita permisos de ubicación si es necesario
                        var permissionStatus = await EnsureLocationPermission();
                        if (permissionStatus != PermissionStatus.Granted)
                        {
                            // Permiso denegado, espera el intervalo antes de reintentar
                            await Task.Delay((int)GpsInterval.TotalMilliseconds, _cts.Token);
                            continue;
                        }

                        // Ejecutar la petición de ubicación en el hilo principal para evitar errores
                        var location = await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            return await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best), _cts.Token);
                        });

                        if (location != null)
                        {
                            // Dispara el evento PositionChanged con la nueva ubicación
                            PositionChanged?.Invoke(this, new LocationEventArgs(new Position()
                            {
                                Guid = Guid.NewGuid().ToString(),
                                Latitude = location.Latitude,
                                Longitude = location.Longitude,
                                Altitude = location.Altitude,
                                Accuracy = location.Accuracy,
                                Course = location.Course,
                                Timestamp = location.Timestamp.UtcDateTime.ToString("o")
                            }));
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Cancellation requested - exit loop
                        break;
                    }
                    finally
                    {
                        await Task.Delay((int)GpsInterval.TotalMilliseconds, _cts.Token);
                    }
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

        private async Task<PermissionStatus> EnsureLocationPermission()
        {
            // Ensure permission requests are invoked on the main thread
            var permissionStatus = await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }

                return status;
            });

            return await Task.FromResult(permissionStatus);
        }
    }
}