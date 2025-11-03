using SQLite;

namespace WayPrecision.Domain.Models
{
    public class Configuration
    {
        [PrimaryKey]
        public string Guid { get; set; }

        public int GpsInterval { get; set; }
        public string AreaUnits { get; set; }
        public string LengthUnits { get; set; }
    }
}