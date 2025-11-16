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

            //registramos el evento para recibir mensajes desde el WebView
            platformView.WebMessageReceived += (s, e) =>
            {
                //obtenemos el mensaje enviado desde el WebView
                var message = e.TryGetWebMessageAsString();

                // enviamos el mensaje al gestor de mensajes
                WebViewMessageManager.EvaluateMessage(message);
            };
        }
    }
}