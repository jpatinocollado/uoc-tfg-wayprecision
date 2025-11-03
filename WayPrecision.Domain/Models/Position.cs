using SQLite;

namespace WayPrecision.Domain.Models
{
    public class Position
    {
        [PrimaryKey]
        public string Guid { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public double Accuracy { get; set; }
        public double Course { get; set; }
        public string Timestamp { get; set; }
    }
}