using WayPrecision.Domain.Models;

namespace WayPrecision.Domain.Helpers.Gps
{
    public interface IGpsFilter
    {
        List<Position> AplyFilter(List<Position> positions);

        bool IsInvalid(Position? last, Position current);
    }
}