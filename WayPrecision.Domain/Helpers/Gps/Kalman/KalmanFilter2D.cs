using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WayPrecision.Domain.Models;

namespace WayPrecision.Domain.Helpers.Gps.Kalman
{
    // Kalman 2D: aplica dos filtros 1D independientes a lat/lon.
    public class KalmanFilter2D
    {
        private readonly KalmanFilter1D kalmanLat;
        private readonly KalmanFilter1D kalmanLon;

        public KalmanFilter2D(double processNoiseVariance = 1e-5, double measurementNoiseVariance = 1e-2)
        {
            kalmanLat = new KalmanFilter1D(processNoiseVariance, measurementNoiseVariance);
            kalmanLon = new KalmanFilter1D(processNoiseVariance, measurementNoiseVariance);
        }

        public Position Update(Position measurement)
        {
            var lat = kalmanLat.Update(measurement.Latitude);
            var lon = kalmanLon.Update(measurement.Longitude);
            return new Position()
            {
                Latitude = lat,
                Longitude = lon,
                Altitude = measurement.Altitude,
                Accuracy = measurement.Accuracy,
                Course = measurement.Course,
                Guid = measurement.Guid,
                Timestamp = measurement.Timestamp
            };
        }

        public void Reset()
        {
            kalmanLat.Reset();
            kalmanLon.Reset();
        }
    }
}