using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WayPrecision.Domain.Sensors.Location
{
    public interface IGpsManager
    {
        Task StartListeningAsync(TimeSpan gpsInterval);

        Task StopListeningAsync();

        Task ChangeGpsInterval(TimeSpan gpsInterval);

        event EventHandler<LocationEventArgs> PositionChanged;
    }
}