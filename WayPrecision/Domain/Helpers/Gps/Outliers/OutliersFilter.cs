using WayPrecision.Domain.Models;

namespace WayPrecision.Domain.Helpers.Gps.Outliers
{
    public class OutliersFilter : IGpsFilter
    {
        private readonly GpsParameters GpsParameters;

        public OutliersFilter(GpsParameters gpsParameters)
        {
            GpsParameters = gpsParameters;
        }

        public List<Position> AplyFilter(List<Position> positions)
        {
            if (positions == null || positions.Count == 0)
                return [];

            //Si esta deshabilitado, retornar la lista original
            if (!GpsParameters.OutliersEnabled)
                return positions;

            // ordenar por timestamp y filtrar outliers
            var outList = new List<Position>();
            Position? last = null;
            foreach (var p in positions.OrderBy(x => x.Timestamp))
            {
                // evaluar si es outlier
                if (IsInvalid(last, p))
                    continue;

                outList.Add(p);
                last = p;
            }

            // retornar lista filtrada
            return outList;
        }

        public bool IsInvalid(Position? last, Position current)
        {
            //si el filtro está deshabilitado, ningún punto es outlier
            if (!GpsParameters.OutliersEnabled)
                return false;

            if (current.Accuracy.HasValue &&
                current.Accuracy.Value > GpsParameters.MinAccuracyMeters &&
                GpsParameters.MinAccuracyMeters > 0)
            {
                return true; // baja precisión
            }

            // si no hay punto previo, no se puede evaluar
            if (last == null)
                return false;

            var dt = (current.Timestamp - last.Timestamp).TotalSeconds;
            if (dt <= 0)
            {
                return true; // timestamps iguales o invertidos
            }
            var dist = last.DistanceTo(current); // metros
            var speed = dist / dt; // m/s
            return dist > GpsParameters.MaxJumpMeters && speed > GpsParameters.MaxAcceptableSpeedMetersPerSec;
        }
    }
}