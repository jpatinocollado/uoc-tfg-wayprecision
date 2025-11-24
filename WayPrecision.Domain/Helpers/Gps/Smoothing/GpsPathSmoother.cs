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
    }
}