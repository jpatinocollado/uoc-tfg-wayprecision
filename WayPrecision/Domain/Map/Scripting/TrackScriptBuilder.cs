using WayPrecision.Domain.Models;

namespace WayPrecision.Domain.Map.Scripting
{
    public class TrackScriptBuilder : MapScriptBuilder
    {
        public TrackScriptBuilder()
        {
        }

        public string GetClearTracks()
        {
            return "TrackManagerService.ClearTracks();";
        }

        public string GetFitTrack(string id)
        {
            return $"TrackManagerService.FitTrack('{id}');";
        }

        public string GetTrack(Track track)
        {
            string coordsLinea = string.Empty;
            string coordsPolygon = string.Empty;

            List<Position> positions = track.TrackPoints.Select(tp => tp.Position).ToList();

            positions.ForEach(a =>
            {
                string lng = a.Longitude.ToString().Replace(",", ".");
                string lat = a.Latitude.ToString().Replace(",", ".");
                string alt = a.Altitude.ToString().Replace(",", ".");

                coordsLinea += $"[{lng},{lat},{alt}]";

                if (positions.IndexOf(a) != positions.Count - 1)
                    coordsLinea += ",";

                if (positions.Last() == a)
                {
                    lat = positions.First().Latitude.ToString().Replace(",", ".");
                    lng = positions.First().Longitude.ToString().Replace(",", ".");
                    alt = positions.First().Altitude.ToString().Replace(",", ".");
                    coordsPolygon = coordsLinea + "," + $"[{lng},{lat},{alt}]";
                    coordsPolygon = $"{coordsLinea},[{lng},{lat},{alt}]";
                }
            });

            int weight = track.IsOpened ? 5 : 2;

            return "TrackManagerService.AddTrack({ " +
                               $"id: '{track.Guid}', " +
                               $"name: '{track.Name}', " +
                               $"description: '{track.Observation}', " +
                               $"visible: {track.IsVisible.ToString().ToLower()}, " +
                               $"length: '{track.LengthLocal}', " +
                               $"area: '{track.AreaLocal}', " +
                               "color: '#31882A', " +
                               "fillColor: '#2AAD27', " +
                               "opacity: 1.0, " +
                               "fillopacity: 0.5, " +
                               $"weight: {weight}, " +
                               $"type: '{track.TypeGeometry.ToString()}', " +
                               "polygon: { " +
                                    "type: 'Feature'," +
                                    "geometry: {" +
                                        "type: 'Polygon'," +
                                        $"coordinates: [[{coordsPolygon}]]" +
                                    "}" +
                                "}," +
                                "lineString: { " +
                                    "type: 'Feature'," +
                                    "geometry: {" +
                                        "type: 'LineString'," +
                                        $"coordinates: [{coordsLinea}]" +
                                    "}" +
                                "}" +
                              " });";
        }
    }
}