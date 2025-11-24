using System.Globalization;
using WayPrecision.Domain.Exceptions;
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
            Context.BtnCreateTrackPublic.Clicked += async (s, e) => await btnCreateTrackClicked(s, e);

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
            Context.BtnCreateTrackPublic.Clicked -= async (s, e) => await btnCreateTrackClicked(s, e);
        }

        /// <summary>
        /// Evento que gestiona la visualización de los datos de ubicación GPS.
        /// </summary>
        /// <param name="sender">Origen del evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        private void BtnGpsDataClicked(object? sender, EventArgs e)
        {
            try
            {
                // Lógica para mostrar los datos de ubicación
                Context.PnGpsDataPublic.IsVisible = !Context.PnGpsDataPublic.IsVisible;
            }
            catch (Exception ex)
            {
                GlobalExceptionManager.HandleException(ex, this.Context);
            }
        }

        /// <summary>
        /// Evento que gestiona la creación de un nuevo waypoint en la ubicación actual.
        /// </summary>
        /// <param name="sender">Origen del evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        private void btnCreateWaypointClicked(object? sender, EventArgs e)
        {
            try
            {
                // Lógica para crear un waypoint
                if (Context._locationEnable && Context._currentPosition != null)
                {
                    DateTime dateTime = DateTime.UtcNow;

                    Waypoint waypoint = new()
                    {
                        Name = "",
                        Observation = "",
                        Created = dateTime.ToString("o"),
                        Position = new Position
                        {
                            Latitude = Context._currentPosition.Latitude,
                            Longitude = Context._currentPosition.Longitude,
                            Accuracy = Context._currentPosition.Accuracy,
                            Altitude = Context._currentPosition.Altitude,
                            Course = Context._currentPosition.Course,
                            Timestamp = dateTime
                        }
                    };

                    Context.Navigation.PushAsync(new WaypointDetailPage(waypoint, DetailPageMode.Created));
                }
                else
                    Context.DisplayAlert("Crear Waypoint", "La ubicación no está habilitada o no se ha obtenido una ubicación válida.", "Aceptar");
            }
            catch (Exception ex)
            {
                GlobalExceptionManager.HandleException(ex, this.Context);
            }
        }

        /// <summary>
        /// Evento que gestiona la transición al estado de seguimiento de track si la ubicación está habilitada.
        /// </summary>
        /// <param name="sender">Origen del evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        private async Task btnCreateTrackClicked(object? sender, EventArgs e)
        {
            try
            {
                Configuration configuration = await _configurationService.GetOrCreateAsync();

                if (configuration.TrackingMode == TrackingModeEnum.GPS.ToString() &&
                    Context._locationEnable &&
                    Context._currentPosition != null)
                    Context.TransitionTo(new MapStateTrackingGps(_service, _configurationService));
                else if (configuration.TrackingMode == TrackingModeEnum.GPS.ToString())
                    Context.DisplayAlert("Crear Track", "La ubicación no está habilitada o no se ha obtenido una ubicación válida.", "Aceptar");
                else
                    Context.TransitionTo(new MapStateTrackingManual(_service, _configurationService));
            }
            catch (Exception ex)
            {
                GlobalExceptionManager.HandleException(ex, this.Context);
            }
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

        public override async Task EvaluateJavascriptMessage(string evento, params string[] args)
        {
            switch (evento)
            {
                case "editWaypoint":
                    string idWaypoint = args.Length > 0 ? args[0] : string.Empty;
                    Context.EditWaypoint(idWaypoint);
                    break;

                case "dblclick":
                    string lat = args.Length > 0 ? args[0] : string.Empty;
                    string lng = args.Length > 1 ? args[1] : string.Empty;

                    double latDouble = 0;
                    double lngDouble = 0;

                    if (double.TryParse(lat, NumberStyles.Float, CultureInfo.InvariantCulture, out double latParsed))
                        latDouble = latParsed;

                    if (double.TryParse(lng, NumberStyles.Float, CultureInfo.InvariantCulture, out double lngParsed))
                        lngDouble = lngParsed;

                    Context.CreateWaypoint(latDouble, lngDouble);
                    break;

                case "editTrack":
                    string idTrack = args.Length > 0 ? args[0] : string.Empty;
                    Context.EditTrack(idTrack);
                    break;

                case "updateTrack":
                    string idUpdatedTrack = args.Length > 0 ? args[0] : string.Empty;
                    double? trackLength = null;
                    double? trackArea = null;
                    double? trackPerimeter = null;

                    if (args.Length > 1 && double.TryParse(args[1],
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture, out double lengthParsed))
                        trackLength = lengthParsed;

                    if (args.Length > 2 && double.TryParse(args[2],
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture, out double areaParsed))
                        trackArea = areaParsed;

                    if (args.Length > 3 && double.TryParse(args[3],
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture, out double perimeterParsed))
                        trackPerimeter = perimeterParsed;

                    await Context.UpdateTrackDataGeometry(idUpdatedTrack, trackLength, trackArea, trackPerimeter);
                    break;
            }

            await Task.CompletedTask;
        }
    }
}