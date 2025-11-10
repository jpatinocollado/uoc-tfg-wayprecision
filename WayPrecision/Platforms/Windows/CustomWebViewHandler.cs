using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;
using WayPrecision.Domain.Components;

namespace WayPrecision
{
    public class CustomWebViewHandler : WebViewHandler
    {
        protected override void ConnectHandler(WebView2 platformView)
        {
            base.ConnectHandler(platformView);

            // Capturar mensajes desde JS -> C#
            platformView.WebMessageReceived += (s, e) =>
            {
                var message = e.TryGetWebMessageAsString();

                // Evaluamos el mensaje recibido para realizar las acciones correspondientes
                WebViewMessageManager.EvaluateMessage(message);
            };
        }

        // Método auxiliar para enviar mensaje desde C# -> JS
        public void SendMessageToJs(string msg)
        {
            if (PlatformView != null)
            {
                PlatformView.ExecuteScriptAsync($"showMessage('{msg}')");
            }
        }
    }
}