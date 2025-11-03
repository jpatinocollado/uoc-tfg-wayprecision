using SQLite;

namespace WayPrecision.Domain.Models
{
    public class Waypoint
    {
        [PrimaryKey]
        public string Guid { get; set; }

        public string Name { get; set; }
        public string Observation { get; set; }
        public string Created { get; set; }

        [Indexed]
        public string Position { get; set; }
    }
}