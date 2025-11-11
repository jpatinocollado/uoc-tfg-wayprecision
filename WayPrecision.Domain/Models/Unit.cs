using SQLite;

namespace WayPrecision.Domain.Models
{
    public class Unit
    {
        [PrimaryKey]
        public string Guid { get; set; } = String.Empty;

        public string Name { get; set; } = String.Empty;
        public string Acronym { get; set; } = String.Empty;
    }
}