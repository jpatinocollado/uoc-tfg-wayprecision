using System;
using SQLite;

namespace WayPrecision.Domain.Models
{
    public class Waypoint
    {
        [PrimaryKey]
        public string Guid { get; set; } = String.Empty;

        public string Name { get; set; } = String.Empty;
        public string Observation { get; set; } = String.Empty;
        public string Created { get; set; } = String.Empty;

        public bool IsVisible { get; set; } = true;

        [Indexed]
        public string PositionGuid { get; set; } = String.Empty;

        [Ignore] // SQLite-net no soporta navegación automática, pero puedes cargarla manualmente
        public virtual Position? Position { get; set; }

        [Ignore]
        public DateTime? CreatedLocal
        {
            get
            {
                if (DateTime.TryParse(Created, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var utcDate))
                {
                    return utcDate.ToLocalTime();
                }
                return null;
            }
        }
    }
}