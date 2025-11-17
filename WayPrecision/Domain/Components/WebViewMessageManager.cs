using System.Globalization;

namespace WayPrecision.Domain.Components
{
    public static class WebViewMessageManager
    {
        public static async Task EvaluateMessage(string message)
        {
            string[] messages = message.Split(';');
            string evento = messages[0];

            if (Application.Current != null &&
                Application.Current.Windows[0].Page is Shell shell &&
                shell.CurrentPage is MainPage mainPage)
            {
                switch (evento)
                {
                    case "mapLoaded":
                        mainPage.SetFirstLoadExecuted();
                        mainPage.PaintElements();
                        break;

                    case "enableLocation":
                        await mainPage.SetEnableLocation(true);
                        break;

                    case "disableLocation":
                        await mainPage.SetEnableLocation(false);
                        break;

                    case "enableCenterLocation":
                        mainPage.SetEnableCenterLocation(true);
                        break;

                    case "disableCenterLocation":
                        mainPage.SetEnableCenterLocation(false);
                        break;

                    case "setZoom":
                        string zoom = messages.Length > 1 ? messages[1] : string.Empty;
                        mainPage.SetZoom(zoom);
                        break;

                    case "editWaypoint":
                        string idWaypoint = messages.Length > 1 ? messages[1] : string.Empty;
                        mainPage.EditWaypoint(idWaypoint);
                        break;

                    case "createWaypoint":
                        string lat = messages.Length > 1 ? messages[1] : string.Empty;
                        string lng = messages.Length > 2 ? messages[2] : string.Empty;

                        double latDouble = 0;
                        double lngDouble = 0;

                        if (double.TryParse(lat, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double latParsed))
                            latDouble = latParsed;

                        if (double.TryParse(lng, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double lngParsed))
                            lngDouble = lngParsed;

                        mainPage.CreateWaypoint(latDouble, lngDouble);
                        break;

                    case "editTrack":
                        string idTrack = messages.Length > 1 ? messages[1] : string.Empty;
                        mainPage.EditTrack(idTrack);
                        break;

                    case "updateTrack":
                        string idUpdatedTrack = messages.Length > 1 ? messages[1] : string.Empty;
                        double? trackLength = null;
                        double? trackArea = null;
                        double? trackPerimeter = null;

                        if (messages.Length > 2 && double.TryParse(messages[2],
                            NumberStyles.Float,
                            CultureInfo.InvariantCulture, out double lengthParsed))
                            trackLength = lengthParsed;

                        if (messages.Length > 3 && double.TryParse(messages[3],
                            NumberStyles.Float,
                            CultureInfo.InvariantCulture, out double areaParsed))
                            trackArea = areaParsed;

                        if (messages.Length > 4 && double.TryParse(messages[4],
                            NumberStyles.Float,
                            CultureInfo.InvariantCulture, out double perimeterParsed))
                            trackPerimeter = perimeterParsed;

                        await mainPage.UpdateTrackDataGeometry(idUpdatedTrack, trackLength, trackArea, trackPerimeter);
                        break;

                    case "setLastCenter":
                    case "setLastZoom":

                        break;

                    default:
                        await mainPage.DisplayAlert("JS -> C#", message, "Aceptar");
                        break;
                }
            }
        }
    }
}