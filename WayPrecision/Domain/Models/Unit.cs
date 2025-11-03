using SQLite;

namespace WayPrecision.Domain.Models
{
    public class Unit
    {
        [PrimaryKey]
        public string Guid { get; set; }

        public string Name { get; set; }
        public string Acronym { get; set; }
    }
}