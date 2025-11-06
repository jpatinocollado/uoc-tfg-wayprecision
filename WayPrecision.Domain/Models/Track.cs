using SQLite;

namespace WayPrecision.Domain.Models
{
    public class Track
    {
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
    }
}