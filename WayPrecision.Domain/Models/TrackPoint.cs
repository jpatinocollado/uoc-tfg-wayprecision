using SQLite;

namespace WayPrecision.Domain.Models
{
    public class TrackPoint
    {
        [PrimaryKey]
        public string Guid { get; set; }

        [Indexed]
        public string TrackGuid { get; set; }

        [Indexed]
        public string PositionGuid { get; set; }

        [Ignore]
        public virtual Position Position { get; set; }
    }
}