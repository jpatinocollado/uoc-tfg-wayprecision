using WayPrecision.Domain.Models;
using WayPrecision.Domain.Pages;
using WayPrecision.Domain.Sensors.Location;
using WayPrecision.Domain.Services;

namespace WayPrecision.Pages.Maps
{
    public class MapStateDefault : MapState
    {
        private readonly IService<Track> _service;

        public MapStateDefault(IService<Track> service)
        {
            _service = service;
        }

        public override void Init()
        {
            //Ponemos visibles los botones del pie de pagina
            MapPage.BtnStackLayoutDefaultPublic.IsVisible = true;
            MapPage.BtnStackLayoutTrackingPublic.IsVisible = false;
            MapPage.pnGpsDataPublic.IsVisible = false;

            //Register buttons events
            MapPage.btnGpsDataPublic.Clicked += BtnGpsDataClicked;
            MapPage.btnCreateWaypointPublic.Clicked += btnCreateWaypointClicked;
            MapPage.btnCreateTrackPublic.Clicked += btnCreateTrackClicked;

            MapPage.PaintElements();
        }

        public override void Close()
        {
            //Unregister buttons events
            MapPage.btnGpsDataPublic.Clicked -= BtnGpsDataClicked;
            MapPage.btnCreateWaypointPublic.Clicked -= btnCreateWaypointClicked;
            MapPage.btnCreateTrackPublic.Clicked -= btnCreateTrackClicked;
        }

        private void BtnGpsDataClicked(object? sender, EventArgs e)
        {
            // Lógica para mostrar los datos de ubicación
            MapPage.pnGpsDataPublic.IsVisible = !MapPage.pnGpsDataPublic.IsVisible;
        }

        private void btnCreateWaypointClicked(object? sender, EventArgs e)
        {
            // Lógica para crear un waypoint
            if (MapPage._locationEnable && MapPage._lastPosition != null)
            {
                DateTime dateTime = DateTime.UtcNow;

                Waypoint waypoint = new()
                {
                    Name = "",
                    Observation = "",
                    Created = dateTime.ToString("o"),
                    Position = new Position
                    {
                        Latitude = MapPage._lastPosition.Latitude,
                        Longitude = MapPage._lastPosition.Longitude,
                        Accuracy = MapPage._lastPosition.Accuracy,
                        Altitude = MapPage._lastPosition.Altitude,
                        Course = MapPage._lastPosition.Course,
                        Timestamp = dateTime.ToString("o"),
                    }
                };

                MapPage.Navigation.PushAsync(new WaypointDetailPage(waypoint, DetailPageMode.Created));
            }
            else
                MapPage.DisplayAlert("Crear Waypoint", "La ubicación no está habilitada o no se ha obtenido una ubicación válida.", "Aceptar");
        }

        private void btnCreateTrackClicked(object? sender, EventArgs e)
        {
            // Lógica para crear un track
            if (MapPage._locationEnable && MapPage._lastPosition != null)
            {
                MapPage.TransitionTo(new MapStateTracking(_service));
            }
            else
                MapPage.DisplayAlert("Crear Track", "La ubicación no está habilitada o no se ha obtenido una ubicación válida.", "Aceptar");
        }

        public override async Task AddPosition(GpsLocation lastPosition)
        {
            //En el estado por defecto no se realiza ninguna acción al agregar una posición

            await Task.CompletedTask;
        }
    }
}