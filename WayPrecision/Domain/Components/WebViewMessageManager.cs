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

                    case "setLastCenter":
                    case "setLastZoom":

                        break;
                }

                await mainPage.State.EvaluateJavascriptMessage(message);
            }
        }
    }
}