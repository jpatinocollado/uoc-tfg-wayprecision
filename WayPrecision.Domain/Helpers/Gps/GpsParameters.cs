using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WayPrecision.Domain.Models;
using WayPrecision.Domain.Services.Configuracion;

namespace WayPrecision.Domain.Helpers.Gps
{
    public class GpsParameters
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

        //// ----------------------------------------------------------
        //// Estimación de precisión si el GPS no la proporciona
        //// ----------------------------------------------------------
        private static double EstimateAccuracy(double intervalSeconds)
        {
            // Estimación basada en comportamiento real típico
            if (intervalSeconds <= 1) return 12;   // muestras muy rápidas → ruido visible
            if (intervalSeconds <= 3) return 8;    // caso ideal caminando
            if (intervalSeconds <= 6) return 10;   // intervalo medio
            return 15;                             // intervalos grandes → menos fidelidad
        }
    }
}