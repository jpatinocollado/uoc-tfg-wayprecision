using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WayPrecision.Domain.Helpers.Gps.Kalman
{
    // Filtro de Kalman 1D (clásico, muy simple)
    public class KalmanFilter1D
    {
        // x: state (estimate)
        // p: estimation error covariance
        // q: process noise variance
        // r: measurement noise variance
        private double x;

        private double p;
        private readonly double q;
        private readonly double r;
        private bool initialized = false;

        public KalmanFilter1D(double processNoiseVariance = 1e-5, double measurementNoiseVariance = 1e-2)
        {
            q = processNoiseVariance;
            r = measurementNoiseVariance;
        }

        public double Update(double measurement)
        {
            if (!initialized)
            {
                x = measurement;
                p = 1.0;
                initialized = true;
                return x;
            }

            // Predict
            p = p + q;

            // Update
            double k = p / (p + r);
            x = x + k * (measurement - x);
            p = (1 - k) * p;

            return x;
        }

        public void Reset()
        {
            initialized = false;
            p = 0;
            x = 0;
        }
    }
}