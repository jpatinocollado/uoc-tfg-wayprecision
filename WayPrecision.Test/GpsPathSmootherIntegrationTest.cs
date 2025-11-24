using WayPrecision.Domain.Helpers.Gps;
using WayPrecision.Domain.Helpers.Gps.Outliers;
using WayPrecision.Domain.Helpers.Gps.Smoothing;
using WayPrecision.Domain.Models;

namespace WayPrecision.Test
{
    public class GpsPathSmootherIntegrationTest
    {
        [Fact]
        public void Smoothing_ShouldReduceNoise_AndPreserveTrackShape()
        {
            // Arrange
            GpsParameters parameters = new GpsParameters()
            {
                MaxAcceptableSpeedMetersPerSec = 3.0,
                MaxJumpMeters = 20,
                MovingAverageWindow = 3,
                ProcessNoiseVariance = 1e-5,
                MeasurementNoiseVariance = 5e-3
            };

            var smoother = new GpsPathSmoother(parameters, new OutliersFilter(parameters));

            var raw = SimulateNoisyTrack();

            // Act
            var smooth = smoother.SmoothBatch(raw);
            var comparison = smoother.ComparePaths(raw, smooth);

            // Assert
            Assert.NotEmpty(raw);
            Assert.NotEmpty(smooth);

            // 1) La ruta suavizada debería tener menor longitud total si había ruido
            Assert.True(comparison.TotalDistanceSmoothMeters <= comparison.TotalDistanceRawMeters * 1.05,
                $"La ruta suavizada debería ser más corta. Raw={comparison.TotalDistanceRawMeters}, Smooth={comparison.TotalDistanceSmoothMeters}");

            // 2) La desviación RMS debe ser razonablemente pequeña (< 10 m)
            Assert.True(comparison.RmsDeviationMeters < 10,
                $"RMS expected < 10m but was {comparison.RmsDeviationMeters}");

            // 3) No debe eliminar demasiados puntos
            Assert.True(smooth.Count > raw.Count * 0.7,
                "El suavizado eliminó demasiados puntos, posible falso positivo de outliers.");
        }

        // -------------------------------------------------------------
        // Simulación de track ruidoso (sin dependencias externas)
        // -------------------------------------------------------------
        private List<Position> SimulateNoisyTrack()
        {
            var rnd = new Random(1234);
            var list = new List<Position>();

            double baseLat = 40.4168;
            double baseLon = -3.7038;
            var now = DateTimeOffset.UtcNow;

            for (int i = 0; i < 150; i++)
            {
                // Trayectoria recta con ligera oscilación
                double lat = baseLat + i * 0.00002;
                double lon = baseLon + Math.Sin(i / 8.0) * 0.00001;

                // Ruido equivalente a ±6 metros aprox
                lat += (rnd.NextDouble() - 0.5) * 0.00005;
                lon += (rnd.NextDouble() - 0.5) * 0.00005;

                var p = new Position
                {
                    Latitude = lat,
                    Longitude = lon,
                    Timestamp = now.AddSeconds(i * 2).UtcDateTime
                };
                list.Add(p);
            }

            // Insertar salto fuerte (outlier)
            list.Insert(40, new Position()
            {
                Latitude = baseLat + 0.05,
                Longitude = baseLon + 0.05,
                Timestamp = now.AddSeconds(40 * 2).UtcDateTime
            });

            return list;
        }
    }
}