using WayPrecision.Domain.Helpers.Gps.Kalman;
using WayPrecision.Domain.Helpers.Gps.MovingAverage;
using WayPrecision.Domain.Helpers.Gps.Outliers;
using WayPrecision.Domain.Models;

namespace WayPrecision.Domain.Helpers.Gps.Smoothing
{
    // Clase principal para suavizar trayectorias
    public class GpsPathSmoother
    {
        private readonly IGpsFilter OutliersFilter;
        private readonly IGpsFilter KalmanFilter;
        private readonly IGpsFilter MovingAverageFilter;

        public GpsPathSmoother(GpsParameters gpsParameters)
        {
            OutliersFilter = new OutliersFilter(gpsParameters);
            KalmanFilter = new KalmanFilter(gpsParameters);
            MovingAverageFilter = new MovingAverageFilter(gpsParameters);
        }

        // Smoother sencillo que recibe una lista de puntos crudos y devuelve su versión suavizada
        public List<Position> SmoothBatch(List<Position> rawPoints)
        {
            if (rawPoints == null || rawPoints.Count == 0) return new List<Position>();

            // 1) Eliminar outliers básicos (saltos imposibles por velocidad o distancia)
            var filtered = OutliersFilter.AplyFilter(rawPoints);

            // 2) Aplicar Kalman 2D en secuencia
            var kalmaned = KalmanFilter.AplyFilter(filtered);

            // 3) Opcional: media móvil sobre lat/lon para extra suavizado (con lag)
            var moved = MovingAverageFilter.AplyFilter(kalmaned);

            return moved;
        }

        // Comparación simple antes/después:
        // - distancia total (raw vs smooth)
        // - desviación RMS entre puntos (por índice)
        public SmoothingComparison ComparePaths(List<Position> raw, List<Position> smooth)
        {
            double totalRaw = PathLengthMeters(raw);
            double totalSmooth = PathLengthMeters(smooth);

            double sumSq = 0;
            int count = 0;

            foreach (var r in raw)
            {
                var nearest = FindNearestSpatial(smooth, r);
                double d = r.DistanceTo(nearest);
                sumSq += d * d;
                count++;
            }

            return new SmoothingComparison
            {
                TotalDistanceRawMeters = totalRaw,
                TotalDistanceSmoothMeters = totalSmooth,
                RmsDeviationMeters = Math.Sqrt(sumSq / count),
                RawPoints = raw.Count,
                SmoothPoints = smooth.Count
            };
        }

        // Cálculo de longitud total de una trayectoria
        private static double PathLengthMeters(IList<Position> pts)
        {
            double sum = 0;
            for (int i = 1; i < pts.Count; i++)
                sum += pts[i - 1].DistanceTo(pts[i]);
            return sum;
        }

        private static Position FindNearestSpatial(List<Position> list, Position target)
        {
            Position best = list[0];
            double bestDist = best.DistanceTo(target);

            foreach (var p in list)
            {
                double d = p.DistanceTo(target);
                if (d < bestDist)
                {
                    best = p;
                    bestDist = d;
                }
            }

            return best;
        }
    }
}