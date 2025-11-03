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
        public string IsOpened { get; set; }

        public int TotalPoints { get; set; }
        public string AreaUnits { get; set; }
        public string LengthUnits { get; set; }

        public double Area { get; set; }
        public double Length { get; set; }
    }
}