using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WayPrecision.Domain.Models;

namespace WayPrecision.Domain.Map.Scripting
{
    public class WaypointScriptBuilder : MapScriptBuilder
    {
        public WaypointScriptBuilder()
        {
        }

        public WaypointScriptBuilder ClearWaypoints()
        {
            Script.Append("WaypointManagerService.ClearWaypoints();");

            return this;
        }

        public MapScriptBuilder FitWaypoint(string id)
        {
            Script.Append($"WaypointManagerService.FitWaypoint('{id}');");

            return this;
        }

        public MapScriptBuilder UpdateWaypoints(List<Waypoint> waypoints)
        {
            waypoints.ForEach(waypoint =>
            {
                string lat = waypoint.Position.Latitude.ToString().Replace(',', '.');
                string lng = waypoint.Position.Longitude.ToString().Replace(',', '.');

                Script.Append("WaypointManagerService.AddWaypoint({ " +
                              $"id: '{waypoint.Guid}', " +
                              $"name: '{waypoint.Name }', " +
                              $"description: '{waypoint.Observation}', " +
                              $"lat: {lat}, " +
                              $"lng: {lng} " +
                              " });");

            });

            return this;
        }
    }
}