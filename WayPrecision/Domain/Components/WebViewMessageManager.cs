using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WayPrecision.Domain.Components
{
    public static class WebViewMessageManager
    {
        public static void EvaluateMessage(string message)
        {
            string[] messages = message.Split(';');
            string evento = messages[0];
            if (Application.Current != null &&
                Application.Current.MainPage is Shell shell &&
                            shell.CurrentPage is MainPage mainPage)
            {
                switch (evento)
                {
                    case "enableLocation":
                        mainPage.SetEnableLocation(true);
                        break;

                    case "disableLocation":
                        mainPage.SetEnableLocation(false);
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

                    case "createWaypoint":
                        string lat = messages.Length > 1 ? messages[1] : string.Empty;
                        string lng = messages.Length > 2 ? messages[2] : string.Empty;
                        string alt = messages.Length > 3 ? messages[3] : string.Empty;

                        double latDouble = 0;
                        double lngDouble = 0;
                        double? altDouble = null;

                        if (double.TryParse(lat, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double latParsed))
                            latDouble = latParsed;

                        if (double.TryParse(lng, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double lngParsed))
                            lngDouble = lngParsed;

                        if (double.TryParse(alt, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double altParsed))
                            altDouble = altParsed;

                        mainPage.CreateWaypoint(latDouble, lngDouble, altDouble);
                        break;

                    default:
                        Application.Current.MainPage.DisplayAlert("JS -> C#", message, "Aceptar");
                        break;
                }
            }
        }
    }
}