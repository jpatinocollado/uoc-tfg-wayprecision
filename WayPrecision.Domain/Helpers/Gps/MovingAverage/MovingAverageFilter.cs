using WayPrecision.Domain.Models;

namespace WayPrecision.Domain.Helpers.Gps.MovingAverage
{
    public class MovingAverageFilter : IGpsFilter
    {
        private readonly GpsParameters GpsParameters;

        public MovingAverageFilter(GpsParameters gpsParameters)
        {
            GpsParameters = gpsParameters;
        }

        public List<Position> AplyFilter(List<Position> positions)
        {
            if (!GpsParameters.MovingAverageEnabled)
                return positions;

            if (GpsParameters.MovingAverageWindow < 1)
                return positions;

            var res = new List<Position>();
            var queue = new Queue<Position>();
            double sumLat = 0, sumLon = 0;
            foreach (var p in positions)
            {
                queue.Enqueue(p);
                sumLat += p.Latitude;
                sumLon += p.Longitude;
                if (queue.Count > GpsParameters.MovingAverageWindow)
                {
                    var rm = queue.Dequeue();
                    sumLat -= rm.Latitude;
                    sumLon -= rm.Longitude;
                }

                int n = queue.Count;
                res.Add(new Position()
                {
                    Latitude = sumLat / n,
                    Longitude = sumLon / n,
                    Accuracy = p.Accuracy,
                    Altitude = p.Altitude,
                    Course = p.Course,
                    Timestamp = p.Timestamp,
                });
            }
            return res;
        }

        public bool IsInvalid(Position? last, Position current)
        {
            throw new NotImplementedException();
        }
    }
}