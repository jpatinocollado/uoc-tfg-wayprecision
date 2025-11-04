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
        public string PositionGuid { get; set; }

        [Ignore] // SQLite-net no soporta navegación automática, pero puedes cargarla manualmente
        public virtual Position Position { get; set; }

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