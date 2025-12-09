using System;
using SQLite;

namespace WayPrecision.Domain.Models
{
    public class Position
    {
        [PrimaryKey]
        public string Guid { get; set; } = String.Empty;

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Altitude { get; set; }
        public double? Accuracy { get; set; }
        public double? Course { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string TimestampString => Timestamp.ToString("o");

        public double DistanceTo(Position other) => HaversineDistance(this.Latitude, this.Longitude, other.Latitude, other.Longitude);

        private static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000; // metros
            double dLat = ToRad(lat2 - lat1);
            double dLon = ToRad(lon2 - lon1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double ToRad(double deg) => deg * Math.PI / 180.0;
    }
}