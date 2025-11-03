using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WayPrecision.Domain.Sensors.Location
{
    public class PositionEventArgs
    {
        public PositionEventArgs(Position position)
        {
            Position = position;
        }

        public Position Position { get; set; }
    }
}