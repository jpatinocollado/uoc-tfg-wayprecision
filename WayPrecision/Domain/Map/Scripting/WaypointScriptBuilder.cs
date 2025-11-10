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
            Script.Append(GetClearWaypoints());

            return this;
        }

        public MapScriptBuilder FitWaypoint(string id)
        {
            Script.Append(GetFitWaypoint(id));

            return this;
        }

        public MapScriptBuilder UpdateWaypoints(List<Waypoint> waypoints)
        {
            waypoints.ForEach(waypoint =>
            {
                Script.Append(GetWaypoint(waypoint));
            });

            return this;
        }

        public string GetClearWaypoints()
        {
            return "WaypointManagerService.ClearWaypoints();";
        }
        public string GetFitWaypoint(string id)
        {
            return $"WaypointManagerService.FitWaypoint('{id}');";
        }
        public string GetWaypoint(Waypoint waypoint)
        {
            string lat = waypoint.Position.Latitude.ToString().Replace(',', '.');
            string lng = waypoint.Position.Longitude.ToString().Replace(',', '.');

            return "WaypointManagerService.AddWaypoint({ " +
                              $"id: '{waypoint.Guid}', " +
                              $"name: '{waypoint.Name}', " +
                              $"description: '{waypoint.Observation}', " +
                              $"lat: {lat}, " +
                              $"lng: {lng} " +
                              " });";
        }
    }
}