using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WayPrecision.Domain.Models
{
    public enum TypeGeometry
    {
        //
        // Resumen:
        //     None specified or custom
        Unspecified = 0,

        //
        // Resumen:
        //     Point
        Point = 1,

        //
        // Resumen:
        //     Line
        LineString = 2,

        //
        // Resumen:
        //     Polygon
        Polygon = 3,

        //
        // Resumen:
        //     MultiPoint
        MultiPoint = 4,

        //
        // Resumen:
        //     MultiPolygon
        MultiPolygon = 5
    }
}