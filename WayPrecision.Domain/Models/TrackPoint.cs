using System;
using SQLite;

namespace WayPrecision.Domain.Models
{
    public class TrackPoint
    {
        [PrimaryKey]
        public string Guid { get; set; } = String.Empty;

        [Indexed]
        public string TrackGuid { get; set; } = String.Empty;

        [Indexed]
        public string PositionGuid { get; set; } = String.Empty;

        [Ignore]
        public virtual Position Position { get; set; } = new Position();
    }
}