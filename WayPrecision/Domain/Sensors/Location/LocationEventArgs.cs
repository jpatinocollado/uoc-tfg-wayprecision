using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WayPrecision.Domain.Sensors.Location
{
    public class LocationEventArgs
    {
        public LocationEventArgs(GpsLocation location)
        {
            Location = location;
        }

        public GpsLocation Location { get; set; }
    }
}