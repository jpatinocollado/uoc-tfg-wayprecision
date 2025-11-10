using SQLite;

namespace WayPrecision.Domain.Models
{
    public class Track
    {
        private Configuration? _configuration;

        public void SetConfiguration(Configuration configuration)
        {
            _configuration = configuration;
        }

        [PrimaryKey]
        public string Guid { get; set; }

        public string Name { get; set; }
        public string Observation { get; set; }
        public string Created { get; set; }
        public string Finalized { get; set; }
        public bool IsOpened { get; set; }

        public int TotalPoints { get; set; }
        public string AreaUnits { get; set; }
        public string LengthUnits { get; set; }

        public double? Area { get; set; }
        public double? Length { get; set; }

        public TypeGeometry TypeGeometry { get; set; }

        [Ignore]
        public string AreaLocal
        {
            get
            {
                if (!Area.HasValue)
                    return string.Empty;

                if (_configuration == null)
                    return $"{Math.Round(Area.Value, 2)} m²";

                if (!string.IsNullOrWhiteSpace(_configuration.AreaUnits) && _configuration.AreaUnits == UnitEnum.KilometrosCuadrados.ToString())
                {
                    double km2 = Area.Value / 1_000_000d;
                    return $"{Math.Round(km2, 4)} km²";
                }

                if (!string.IsNullOrWhiteSpace(_configuration.AreaUnits) && _configuration.AreaUnits == UnitEnum.Hectareas.ToString())
                {
                    double ha = Area.Value / 10_000d;
                    return $"{Math.Round(ha, 4)} ha";
                }

                //return fault metros cuadrados
                return $"{Math.Round(Area.Value, 2)} m²";
            }
        }

        [Ignore]
        public string LengthLocal
        {
            get
            {
                if (!Length.HasValue)
                    return string.Empty;

                if (_configuration == null)
                    return $"{Math.Round(Length.Value, 2)} m";

                if (!string.IsNullOrWhiteSpace(_configuration.LengthUnits) && _configuration.LengthUnits == UnitEnum.Kilometros.ToString())
                {
                    double km = Length.Value / 1_000d;
                    return $"{Math.Round(km, 4)} km";
                }

                return $"{Math.Round(Length.Value, 2)} m";
            }
        }

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

        [Ignore]
        public DateTime? FinalizedLocal
        {
            get
            {
                if (DateTime.TryParse(Finalized, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var utcDate))
                {
                    return utcDate.ToLocalTime();
                }
                return null;
            }
        }

        [Ignore]
        public TimeSpan Duration
        {
            get
            {
                if (CreatedLocal.HasValue && FinalizedLocal.HasValue)
                {
                    return FinalizedLocal.Value - CreatedLocal.Value;
                }
                return TimeSpan.Zero;
            }
        }

        [Ignore]
        public virtual List<TrackPoint> TrackPoints { get; set; } = new List<TrackPoint>();
    }
}