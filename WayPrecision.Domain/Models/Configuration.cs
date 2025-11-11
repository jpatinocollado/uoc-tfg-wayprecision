using SQLite;

namespace WayPrecision.Domain.Models
{
    public class Configuration
    {
        [PrimaryKey]
        public string Guid { get; set; } = String.Empty;

        public int GpsInterval { get; set; }
        public string AreaUnits { get; set; } = String.Empty;
        public string LengthUnits { get; set; } = String.Empty;
    }
}