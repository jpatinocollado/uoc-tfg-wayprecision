using WayPrecision.Domain.Helpers.Gps.Kalman;
using WayPrecision.Domain.Models;

namespace WayPrecision.Domain.Helpers.Gps.Smoothing
{
    // Clase principal para suavizar trayectorias
    public class GpsPathSmoother
    {
        // Parámetros configurables
        public int MinAccuracyMeters { get; set; } = 10; // precisión mínima aceptable

        public double MaxAcceptableSpeedMetersPerSec { get; set; } = 3.0; // caminar

        public double MaxJumpMeters { get; set; } = 10.0; // saltos razonables
        public int MovingAverageWindow { get; set; } = 5; // más estable
        public double ProcessNoiseVariance { get; set; } = 5e-4; // movimiento suave real
        public double MeasurementNoiseVariance { get; set; } = 8e-5; // ruido GPS realista
        public bool OutliersEnabled { get; set; } = true;
        public bool MovingAverageEnabled { get; set; } = true;
        public bool KalmanEnabled { get; set; } = true;

        public void UpdateParameters(double intervalSeconds, double? horizontalAccuracyMeters = null)
        {
            // ---------- Normalización del intervalo ----------
            intervalSeconds = Math.Clamp(intervalSeconds, 0.2, 20.0);

            // ---------- Precisión opcional ----------
            // Si no viene dada, estimamos un valor razonable según el intervalo.
            // - Intervalos cortos: más ruido aparente → precisión estimada ligeramente peor.
            // - Intervalos largos: el ruido relativo baja.
            double accuracy = horizontalAccuracyMeters ?? EstimateAccuracy(intervalSeconds);

            // Limitar valores extremos
            accuracy = Math.Clamp(accuracy, 2, 60);

            // ======================================================
            // 1. Velocidad máxima aceptada
            // ======================================================
            if (intervalSeconds <= 3)
                MaxAcceptableSpeedMetersPerSec = 3.0;
            else if (intervalSeconds <= 7)
                MaxAcceptableSpeedMetersPerSec = 4.0;
            else
                MaxAcceptableSpeedMetersPerSec = 5.0;

            // ======================================================
            // 2. MaxJumpMeters
            // ======================================================
            double expectedDistance = intervalSeconds * 1.2; // caminando
            MaxJumpMeters = expectedDistance + (accuracy * 1.5);
            MaxJumpMeters = Math.Clamp(MaxJumpMeters, 8, 60);

            // ======================================================
            // 3. Media móvil
            // ======================================================
            if (intervalSeconds <= 2)
                MovingAverageWindow = 5;
            else if (intervalSeconds <= 5)
                MovingAverageWindow = 3;
            else
                MovingAverageWindow = 2;

            if (accuracy > 20) // GPS degradado → más suavizado
                MovingAverageWindow = Math.Max(3, MovingAverageWindow + 1);

            // ======================================================
            // 4. Process Noise (Q)
            // ======================================================
            double movementFactor = Math.Pow(intervalSeconds, 0.7);
            double accuracyFactor = accuracy / 10.0;

            ProcessNoiseVariance = 4e-4 * movementFactor * accuracyFactor;
            ProcessNoiseVariance = Math.Clamp(ProcessNoiseVariance, 5e-4, 5e-3);

            // ======================================================
            // 5. Measurement Noise (R)
            // ======================================================
            double baseR = (accuracy * 0.5) / 10.0;

            MeasurementNoiseVariance = baseR * (1.0 / Math.Sqrt(intervalSeconds));
            MeasurementNoiseVariance = Math.Clamp(MeasurementNoiseVariance, 3e-5, 5e-4);
        }

        // Smoother sencillo que recibe una lista de puntos crudos y devuelve su versión suavizada
        public List<Position> SmoothBatch(List<Position> rawPoints)
        {
            if (rawPoints == null || rawPoints.Count == 0) return new List<Position>();

            // 1) Eliminar outliers básicos (saltos imposibles por velocidad o distancia)
            var filtered = OutliersEnabled ? RemoveOutliers(rawPoints) : rawPoints;

            // 2) Aplicar Kalman 2D en secuencia
            var kalmaned = new List<Position>(filtered.Count);
            if (KalmanEnabled)
            {
                var k = new KalmanFilter2D(ProcessNoiseVariance, MeasurementNoiseVariance);
                foreach (var p in filtered)
                    kalmaned.Add(k.Update(p));
            }
            else
                kalmaned = filtered;

            // 3) Opcional: media móvil sobre lat/lon para extra suavizado (con lag)
            if (MovingAverageEnabled && MovingAverageWindow > 1)
                return MovingAverage(kalmaned, MovingAverageWindow);

            return kalmaned;
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
        public static double PathLengthMeters(IList<Position> pts)
        {
            double sum = 0;
            for (int i = 1; i < pts.Count; i++)
                sum += pts[i - 1].DistanceTo(pts[i]);
            return sum;
        }

        // ----------------------------------------------------------
        // Estimación de precisión si el GPS no la proporciona
        // ----------------------------------------------------------
        private static double EstimateAccuracy(double intervalSeconds)
        {
            // Estimación basada en comportamiento real típico
            if (intervalSeconds <= 1) return 12;   // muestras muy rápidas → ruido visible
            if (intervalSeconds <= 3) return 8;    // caso ideal caminando
            if (intervalSeconds <= 6) return 10;   // intervalo medio
            return 15;                             // intervalos grandes → menos fidelidad
        }

        // Elimina puntos con salto improbable comparado con tiempo transcurrido
        private List<Position> RemoveOutliers(List<Position> input)
        {
            var outList = new List<Position>();
            Position? last = null;
            foreach (var p in input.OrderBy(x => x.Timestamp))
            {
                if (last == null)
                {
                    outList.Add(p);
                    last = p;
                    continue;
                }

                if (p.Accuracy.HasValue && p.Accuracy.Value > MinAccuracyMeters)
                {
                    // descartar p por baja precisión
                    continue;
                }

                var dt = (p.Timestamp - last.Timestamp).TotalSeconds;
                if (dt <= 0) // ignore or keep with small positive dt
                {
                    // si timestamps iguales o invertidos, ignora el nuevo
                    continue;
                }

                var dist = last.DistanceTo(p); // metros
                var speed = dist / dt; // m/s

                // condición de outlier: distancia demasiado grande o velocidad imposible
                if (dist > MaxJumpMeters && speed > MaxAcceptableSpeedMetersPerSec)
                {
                    // descartar p
                    continue;
                }

                outList.Add(p);
                last = p;
            }

            return outList;
        }

        // Media móvil simple (causal)
        private List<Position> MovingAverage(List<Position> pts, int window)
        {
            var res = new List<Position>();
            var queue = new Queue<Position>();
            double sumLat = 0, sumLon = 0;
            foreach (var p in pts)
            {
                queue.Enqueue(p);
                sumLat += p.Latitude;
                sumLon += p.Longitude;
                if (queue.Count > window)
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