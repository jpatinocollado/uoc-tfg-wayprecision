using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WayPrecision.Domain.Models
{
    public class LocationEventArgs
    {
        public LocationEventArgs(Position position)
        {
            Position = position;
        }

        public Position Position { get; set; }
    }
}