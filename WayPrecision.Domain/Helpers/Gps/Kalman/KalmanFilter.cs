using System.Collections.Generic;
using WayPrecision.Domain.Models;

namespace WayPrecision.Domain.Helpers.Gps.Kalman
{
    public class KalmanFilter : IGpsFilter
    {
        private readonly GpsParameters GpsParameters;

        public KalmanFilter(GpsParameters gpsParameters)
        {
            GpsParameters = gpsParameters;
        }

        public List<Position> AplyFilter(List<Position> positions)
        {
            if (!GpsParameters.KalmanEnabled)
                return positions;

            // 2) Aplicar Kalman 2D en secuencia
            var kalmaned = new List<Position>(positions.Count);
            if (GpsParameters.KalmanEnabled)
            {
                var k = new KalmanFilter2D(GpsParameters.ProcessNoiseVariance, GpsParameters.MeasurementNoiseVariance);
                foreach (var p in positions)
                    kalmaned.Add(k.Update(p));
            }

            return kalmaned;
        }

        public bool IsInvalid(Position? last, Position current)
        {
            return false;
        }
    }
}