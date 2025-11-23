using WayPrecision.Domain.Models;

namespace WayPrecision.Pages.Maps
{
    /// <summary>
    /// Representa el estado base abstracto para la gestión de mapas en la aplicación.
    /// </summary>
    public abstract class MapState : IDisposable
    {
        private bool disposedValue;

        /// <summary>
        /// Página principal asociada al estado del mapa.
        /// </summary>
        protected MainPage? MapPage = null;

        internal MainPage Context => MapPage ?? throw new InvalidOperationException("El contexto del mapa no ha sido establecido.");

        /// <summary>
        /// Asocia el contexto de la página principal al estado del mapa.
        /// </summary>
        /// <param name="mapPage">Instancia de <see cref="MainPage"/> a asociar.</param>
        public void SetContext(MainPage mapPage)
        {
            MapPage = mapPage;
        }

        /// <summary>
        /// Inicializa el estado del mapa.
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// Cierra y limpia el estado del mapa.
        /// </summary>
        public abstract void Close();

        /// <summary>
        /// Agrega una nueva posición GPS al estado del mapa.
        /// </summary>
        /// <param name="lastPosition">Última posición GPS obtenida.</param>
        public abstract Task AddPosition(Position lastPosition);

        public Task EvaluateJavascriptMessage(string message)
        {
            string[] messages = message.Split(';');
            string evento = messages[0];
            string[] args = [.. messages.Skip(1)];

            return EvaluateJavascriptMessage(evento, args);
        }

        public abstract Task EvaluateJavascriptMessage(string evento, params string[] args);

        /// <summary>
        /// Libera los recursos utilizados por la instancia.
        /// </summary>
        /// <param name="disposing">Indica si se deben liberar los recursos administrados.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //eliminar el estado administrado (objetos administrados)
                }

                Close();
                disposedValue = true;
            }
        }

        /// <summary>
        /// Libera los recursos utilizados por la instancia y suprime la finalización.
        /// </summary>
        public void Dispose()
        {
            // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}