using Android.Webkit;
using Microsoft.Maui.Handlers;
using WayPrecision.Domain.Components;

namespace WayPrecision
{
    public class CustomWebViewHandler : WebViewHandler
    {
        protected override void ConnectHandler(Android.Webkit.WebView platformView)
        {
            base.ConnectHandler(platformView);

            // Habilitar JavaScript
            platformView.Settings.JavaScriptEnabled = true;

            // Registrar JS Bridge
            platformView.AddJavascriptInterface(new JSBridge(), "jsBridge");
        }

        public class JSBridge : Java.Lang.Object
        {
            [JavascriptInterface]
            [Java.Interop.Export("postMessage")]
            public void PostMessage(string message)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // enviamos el mensaje al gestor de mensajes
                    WebViewMessageManager.EvaluateMessage(message);
                });
            }
        }
    }
}