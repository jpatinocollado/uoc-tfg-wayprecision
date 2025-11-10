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
                // Ejecutar en hilo principal
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    WebViewMessageManager.EvaluateMessage(message);
                });
            }
        }

        // Método para enviar mensaje desde C# -> JS
        public void SendMessageToJs(string msg)
        {
            PlatformView?.EvaluateJavascript($"showMessage('{msg}')", null);
        }
    }
}