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
            string datos = messages.Length > 1 ? messages[1] : string.Empty;
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
                        mainPage.SetZoom(datos);
                        break;

                    default:
                        Application.Current.MainPage.DisplayAlert("JS -> C#", message, "Aceptar");
                        break;
                }
            }
        }
    }
}