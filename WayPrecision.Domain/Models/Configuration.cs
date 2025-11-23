using SQLite;

namespace WayPrecision.Domain.Models
{
    public class Configuration
    {
        [PrimaryKey]
        public string Guid { get; set; } = String.Empty;

        public int GpsInterval { get; set; } = 3;

        public int GpsAccuracy { get; set; } = 10;

        public string AreaUnits { get; set; } = String.Empty;

        public string LengthUnits { get; set; } = String.Empty;

        public bool KalmanFilterEnabled { get; set; } = true;
        public bool MovingAverageFilterEnabled { get; set; } = true;
        public bool OutliersFilterEnabled { get; set; } = true;
    }
}