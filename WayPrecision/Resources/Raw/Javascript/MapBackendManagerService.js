let MapBackendManagerService = (function () {
    let postMessage = function (message) {
        if (window.chrome && window.chrome.webview) {
            // Windows
            window.chrome.webview.postMessage(message);
        } else if (window.external && window.external.notify) {
            // Android
            window.external.notify(message);
        } else if (typeof jsBridge !== "undefined") {
            // Android con AddJavascriptInterface
            // Android
            jsBridge.postMessage(message);
        } else {
            alert("No se encontró un canal de comunicación con .NET");
        }
    }

    //return plublic methods of service
    return {
        PostMessage: postMessage
    };
})();