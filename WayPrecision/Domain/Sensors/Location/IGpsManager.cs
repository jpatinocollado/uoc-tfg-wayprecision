using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WayPrecision.Domain.Sensors.Location
{
    public interface IGpsManager
    {
        Task StartListeningAsync();

        Task StopListeningAsync();

        event EventHandler<PositionEventArgs> PositionChanged;
    }
}