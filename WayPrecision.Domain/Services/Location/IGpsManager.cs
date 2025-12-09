using System;
using System.Threading.Tasks;
using WayPrecision.Domain.Models;

namespace WayPrecision.Domain.Services.Location
{
    public interface IGpsManager
    {
        Task StartListeningAsync(TimeSpan gpsInterval);

        Task StopListeningAsync();

        Task ChangeGpsInterval(TimeSpan gpsInterval);

        event EventHandler<LocationEventArgs> PositionChanged;
    }
}