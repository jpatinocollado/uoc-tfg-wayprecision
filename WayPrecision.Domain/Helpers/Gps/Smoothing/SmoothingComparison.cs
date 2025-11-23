using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WayPrecision.Domain.Helpers.Gps.Smoothing
{
    public class SmoothingComparison
    {
        public double TotalDistanceRawMeters { get; set; }
        public double TotalDistanceSmoothMeters { get; set; }
        public double RmsDeviationMeters { get; set; }
        public int RawPoints { get; set; }
        public int SmoothPoints { get; set; }

        public override string ToString()
        {
            return $"Raw: {RawPoints} pts, Smooth: {SmoothPoints} pts, DistRaw={TotalDistanceRawMeters:F1} m, DistSmooth={TotalDistanceSmoothMeters:F1} m, RMSDev={RmsDeviationMeters:F2} m";
        }
    }
}