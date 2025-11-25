using WayPrecision.Domain.Models;

namespace WayPrecision.Domain.Map.Scripting
{
    public class WaypointScriptBuilder : MapScriptBuilder
    {
        public WaypointScriptBuilder()
        {
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
                              $"visible: {waypoint.IsVisible.ToString().ToLower()}, " +
                              $"lat: {lat}, " +
                              $"lng: {lng} " +
                              " });";
        }
    }
}