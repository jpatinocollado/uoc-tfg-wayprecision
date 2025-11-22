using WayPrecision.Domain.Models;
using WayPrecision.Domain.Pages;
using WayPrecision.Domain.Services;
using WayPrecision.Domain.Services.Configuracion;

namespace WayPrecision.Pages.Maps
{
    /// <summary>
    /// Estado por defecto del mapa. Gestiona la visualización y eventos principales cuando no se está realizando un seguimiento.
    /// </summary>
    public class MapStateDefault : MapState
    {
        private readonly IService<Track> _service;
        private readonly IConfigurationService _configurationService;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="MapStateDefault"/>.
        /// </summary>
        /// <param name="service">Servicio para la gestión de tracks.</param>
        public MapStateDefault(IService<Track> service, IConfigurationService configurationService)
        {
            _service = service;
            _configurationService = configurationService;
        }

        /// <summary>
        /// Inicializa el estado por defecto del mapa, mostrando los controles y registrando los eventos de los botones.
        /// </summary>
        public override void Init()
        {
            //desbloqueamos el menú
            Shell.SetNavBarIsVisible(MapPage, true);
            Shell.SetFlyoutBehavior(MapPage, FlyoutBehavior.Flyout);

            //Ponemos visibles los botones del pie de pagina
            Context.BtnStackLayoutDefaultPublic.IsVisible = true;
            //Ocultamos botones de tracking
            Context.BtnStackLayoutTrackingPublic.IsVisible = false;
            Context.PnGpsDataPublic.IsVisible = false;

            //Registramos los eventos de los botones
            Context.BtnGpsDataPublic.Clicked += BtnGpsDataClicked;
            Context.BtnCreateWaypointPublic.Clicked += btnCreateWaypointClicked;
            Context.BtnCreateTrackPublic.Clicked += btnCreateTrackClicked;

            //dibujamos los elementos en el mapa
            Context.PaintElements();
        }

        /// <summary>
        /// Cierra el estado por defecto del mapa, desregistrando los eventos de los botones.
        /// </summary>
        public override void Close()
        {
            //Unregister buttons events
            Context.BtnGpsDataPublic.Clicked -= BtnGpsDataClicked;
            Context.BtnCreateWaypointPublic.Clicked -= btnCreateWaypointClicked;
            Context.BtnCreateTrackPublic.Clicked -= btnCreateTrackClicked;
        }

        /// <summary>
        /// Evento que gestiona la visualización de los datos de ubicación GPS.
        /// </summary>
        /// <param name="sender">Origen del evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        private void BtnGpsDataClicked(object? sender, EventArgs e)
        {
            // Lógica para mostrar los datos de ubicación
            Context.PnGpsDataPublic.IsVisible = !Context.PnGpsDataPublic.IsVisible;
        }

        /// <summary>
        /// Evento que gestiona la creación de un nuevo waypoint en la ubicación actual.
        /// </summary>
        /// <param name="sender">Origen del evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        private void btnCreateWaypointClicked(object? sender, EventArgs e)
        {
            // Lógica para crear un waypoint
            if (Context._locationEnable && Context._lastPosition != null)
            {
                DateTime dateTime = DateTime.UtcNow;

                Waypoint waypoint = new()
                {
                    Name = "",
                    Observation = "",
                    Created = dateTime.ToString("o"),
                    Position = new Position
                    {
                        Latitude = Context._lastPosition.Latitude,
                        Longitude = Context._lastPosition.Longitude,
                        Accuracy = Context._lastPosition.Accuracy,
                        Altitude = Context._lastPosition.Altitude,
                        Course = Context._lastPosition.Course,
                        Timestamp = dateTime
                    }
                };

                Context.Navigation.PushAsync(new WaypointDetailPage(waypoint, DetailPageMode.Created));
            }
            else
                Context.DisplayAlert("Crear Waypoint", "La ubicación no está habilitada o no se ha obtenido una ubicación válida.", "Aceptar");
        }

        /// <summary>
        /// Evento que gestiona la transición al estado de seguimiento de track si la ubicación está habilitada.
        /// </summary>
        /// <param name="sender">Origen del evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        private void btnCreateTrackClicked(object? sender, EventArgs e)
        {
            // Lógica para crear un track
            if (Context._locationEnable && Context._lastPosition != null)
            {
                Context.TransitionTo(new MapStateTracking(_service, _configurationService));
            }
            else
                Context.DisplayAlert("Crear Track", "La ubicación no está habilitada o no se ha obtenido una ubicación válida.", "Aceptar");
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