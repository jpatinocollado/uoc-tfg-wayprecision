using WayPrecision.Domain.Models;
using WayPrecision.Domain.Pages;
using WayPrecision.Domain.Sensors.Location;
using WayPrecision.Domain.Services;

namespace WayPrecision.Pages.Maps
{
    /// <summary>
    /// Estado por defecto del mapa. Gestiona la visualización y eventos principales cuando no se está realizando un seguimiento.
    /// </summary>
    public class MapStateDefault : MapState
    {
        private readonly IService<Track> _service;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="MapStateDefault"/>.
        /// </summary>
        /// <param name="service">Servicio para la gestión de tracks.</param>
        public MapStateDefault(IService<Track> service)
        {
            _service = service;
        }

        /// <summary>
        /// Inicializa el estado por defecto del mapa, mostrando los controles y registrando los eventos de los botones.
        /// </summary>
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

        /// <summary>
        /// Cierra el estado por defecto del mapa, desregistrando los eventos de los botones.
        /// </summary>
        public override void Close()
        {
            //Unregister buttons events
            MapPage.btnGpsDataPublic.Clicked -= BtnGpsDataClicked;
            MapPage.btnCreateWaypointPublic.Clicked -= btnCreateWaypointClicked;
            MapPage.btnCreateTrackPublic.Clicked -= btnCreateTrackClicked;
        }

        /// <summary>
        /// Evento que gestiona la visualización de los datos de ubicación GPS.
        /// </summary>
        /// <param name="sender">Origen del evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        private void BtnGpsDataClicked(object? sender, EventArgs e)
        {
            // Lógica para mostrar los datos de ubicación
            MapPage.pnGpsDataPublic.IsVisible = !MapPage.pnGpsDataPublic.IsVisible;
        }

        /// <summary>
        /// Evento que gestiona la creación de un nuevo waypoint en la ubicación actual.
        /// </summary>
        /// <param name="sender">Origen del evento.</param>
        /// <param name="e">Argumentos del evento.</param>
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

        /// <summary>
        /// Evento que gestiona la transición al estado de seguimiento de track si la ubicación está habilitada.
        /// </summary>
        /// <param name="sender">Origen del evento.</param>
        /// <param name="e">Argumentos del evento.</param>
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

        /// <summary>
        /// En el estado por defecto no se realiza ninguna acción al agregar una posición GPS.
        /// </summary>
        /// <param name="lastPosition">Última posición GPS obtenida.</param>
        public override async Task AddPosition(Position lastPosition)
        {
            //En el estado por defecto no se realiza ninguna acción al agregar una posición

            await Task.CompletedTask;
        }
    }
}