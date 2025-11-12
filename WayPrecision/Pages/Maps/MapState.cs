using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WayPrecision.Domain.Sensors.Location;

namespace WayPrecision.Pages.Maps
{
    public abstract class MapState : IDisposable
    {
        private bool disposedValue;

        protected MainPage? MapPage = null;

        public void SetContext(MainPage mapPage)
        {
            MapPage = mapPage;
        }

        public abstract void Init();

        public abstract void Close();

        public abstract Task AddPosition(GpsLocation lastPosition);

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

        public void Dispose()
        {
            // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}