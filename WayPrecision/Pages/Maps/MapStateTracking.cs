using WayPrecision.Domain.Exceptions;
using WayPrecision.Domain.Helpers.Gps.Smoothing;
using WayPrecision.Domain.Map.Scripting;
using WayPrecision.Domain.Models;
using WayPrecision.Domain.Services;
using WayPrecision.Domain.Services.Configuracion;

namespace WayPrecision.Pages.Maps
{
    /// <summary>
    /// Estado del mapa encargado de gestionar la grabación y seguimiento de un track GPS.
    /// </summary>
    public class MapStateTracking : MapState
    {
        private readonly TrackScriptBuilder _trackScriptBuilder;
        private readonly IService<Track> _service;
        private readonly IConfigurationService _configurationService;
        private Configuration configuration;
        private GpsPathSmoother _gpsPathSmoother;

        private Track CurrentTrack;
        private bool IsListening = false;

        private Position? LastPosition = null;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="MapStateTracking"/>.
        /// </summary>
        /// <param name="service">Servicio para la gestión de tracks.</param>
        public MapStateTracking(IService<Track> service, IConfigurationService configurationService)
        {
            _trackScriptBuilder = new TrackScriptBuilder();
            _configurationService = configurationService;
            _service = service;
        }

        /// <summary>
        /// Inicializa el estado de seguimiento, crea el track y configura los controles y eventos.
        /// </summary>
        public override async void Init()
        {
            configuration = await _configurationService.GetOrCreateAsync();
            _gpsPathSmoother = new GpsPathSmoother
            {
                OutliersEnabled = configuration.OutliersFilterEnabled,
                MovingAverageEnabled = configuration.MovingAverageFilterEnabled,
                KalmanEnabled = configuration.KalmanFilterEnabled,
                MinAccuracyMeters = configuration.GpsAccuracy,
                MaxAcceptableSpeedMetersPerSec = 3.0,  // caminar
                MaxJumpMeters = 10,                    // saltos razonables
                MovingAverageWindow = 5,                // más estable
                ProcessNoiseVariance = 5e-4,           // movimiento suave real
                MeasurementNoiseVariance = 8e-5        // ruido GPS realista
            };

            //Crea una nueva instancia de Track
            CurrentTrack = new()
            {
                Guid = Guid.NewGuid().ToString(),
                TypeGeometry = TypeGeometry.LineString,
                Created = DateTime.UtcNow.ToString("o"),
                TrackPoints = new List<TrackPoint>(),
                IsOpened = true
            };

            //Mostramos el total de puntos
            Context.LbTotalPointsPublic.Text = "Puntos: 0";

            //Bloqueamos el menú
            Shell.SetNavBarIsVisible(MapPage, true);
            Shell.SetFlyoutBehavior(MapPage, FlyoutBehavior.Disabled);

            //Ponemos visibles los botones del pie de pagina
            Context.BtnStackLayoutDefaultPublic.IsVisible = false;
            Context.BtnStackLayoutTrackingPublic.IsVisible = true;
            Context.PnGpsDataPublic.IsVisible = true;

            //Registramos los eventos de los botones
            Context.BtnPlayPublic.Clicked += OnPlayClicked;
            Context.BtnPausePublic.Clicked += OnPauseClicked;
            Context.BtnStopPublic.Clicked += OnStopClicked;
            Context.BtnCancelPublic.Clicked += OnCancelClicked;

            //Por defecto entramos en modo Play
            Context.BtnPlayPublic.IsEnabled = false;
            Context.BtnPausePublic.IsEnabled = true;
            Context.BtnStopPublic.IsEnabled = true;

            //Comenzamos a escuchar posiciones GPS
            IsListening = true;

            //Limpiamos el mapa de elementos para visualizar solo el Track
            Context.ClearElements();
        }

        /// <summary>
        /// Cierra el estado de seguimiento, desregistrando los eventos de los botones.
        /// </summary>
        public override void Close()
        {
            //Unregister buttons events
            Context.BtnPlayPublic.Clicked -= OnPlayClicked;
            Context.BtnPausePublic.Clicked -= OnPauseClicked;
            Context.BtnStopPublic.Clicked -= OnStopClicked;
            Context.BtnCancelPublic.Clicked -= OnCancelClicked;
        }

        /// <summary>
        /// Evento que inicia la grabación del track y actualiza el estado de los controles.
        /// </summary>
        /// <param name="sender">Origen del evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        private void OnPlayClicked(object? sender, EventArgs e)
        {
            try
            {
                //start listening GPS
                IsListening = true;

                //Update buttons state
                Context.BtnPlayPublic.IsEnabled = false;
                Context.BtnPausePublic.IsEnabled = true;
                Context.BtnStopPublic.IsEnabled = true;
            }
            catch (Exception ex)
            {
                GlobalExceptionManager.HandleException(ex, this.Context);
            }
        }

        /// <summary>
        /// Evento que pausa la grabación del track y actualiza el estado de los controles.
        /// </summary>
        /// <param name="sender">Origen del evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        private void OnPauseClicked(object? sender, EventArgs e)
        {
            try
            {
                //stop listening GPS
                IsListening = false;

                //Update buttons state
                Context.BtnPlayPublic.IsEnabled = true;
                Context.BtnPausePublic.IsEnabled = false;
                Context.BtnStopPublic.IsEnabled = true;
            }
            catch (Exception ex)
            {
                GlobalExceptionManager.HandleException(ex, this.Context);
            }
        }

        /// <summary>
        /// Evento que finaliza el track actual, solicita información al usuario y guarda el track.
        /// </summary>
        /// <param name="sender">Origen del evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        private async void OnStopClicked(object? sender, EventArgs e)
        {
            try
            {
                //al hacer stop, procedemos a finalizar el track, lo primero es ponerlo en pausa
                OnPauseClicked(null, new EventArgs());

                //Finalize current track
                CurrentTrack.Finalized = DateTime.UtcNow.ToString("o");

                // Pregunta al usuario si quiere cerrar el track
                bool cerrarTrack = await Context.DisplayAlert(
                        "Finalizar Track",
                        "¿Quieres cerrar el track?",
                        "Sí",
                        "No"
                    );

                if (cerrarTrack)
                {
                    CurrentTrack.IsOpened = false;
                    CurrentTrack.TypeGeometry = TypeGeometry.Polygon;
                }
                else
                {
                    CurrentTrack.IsOpened = true;
                    CurrentTrack.TypeGeometry = TypeGeometry.LineString;
                }

                string nameTrack = string.Empty;

                while (string.IsNullOrWhiteSpace(nameTrack))
                    nameTrack = await Context.DisplayPromptAsync("Nombre del Track", "Introduce el nombre del track:", accept: "Aceptar", cancel: "Cancelar", maxLength: 50);

                CurrentTrack.Name = nameTrack;
                CurrentTrack = await _service.CreateAsync(CurrentTrack);

                await Context.ShowLoading("Calculando <br/> geometrías...");

                Context.TransitionTo(new MapStateDefault(_service, _configurationService));

                // espera 10 segundos para que se calculen las medidas
                await Task.Delay(4000);

                await Context.HideLoading();

                Context.EditTrack(CurrentTrack.Guid);
            }
            catch (Exception ex)
            {
                GlobalExceptionManager.HandleException(ex, this.Context);
            }
            finally
            {
                await Context.HideLoading();
            }
        }

        /// <summary>
        /// Evento que cancela la grabación del track actual, preguntando al usuario y gestionando la transición de estado.
        /// </summary>
        /// <param name="sender">Origen del evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        private async void OnCancelClicked(object? sender, EventArgs e)
        {
            try
            {
                bool reanudarTrack = IsListening;

                //al cancelar el track, lo primero es ponerlo en pausa
                OnPauseClicked(null, new EventArgs());

                //preguntamos si quiere cancelar el Track
                bool cancelarTrack = await Context.DisplayAlert(
                    "Finalizar Track",
                    "¿Quieres Cancelar el track?",
                    "Sí",
                    "No"
                );

                if (cancelarTrack)
                {
                    //Hacemos la transición de estado sin guardar el track
                    Context.TransitionTo(new MapStateDefault(_service, _configurationService));
                }
                else
                {
                    //Si hay que reanudar el Track simulamos pulsar Pause
                    if (reanudarTrack)
                        OnPlayClicked(null, new EventArgs());
                }
            }
            catch (Exception ex)
            {
                GlobalExceptionManager.HandleException(ex, this.Context);
            }
        }

        /// <summary>
        /// Agrega una nueva posición GPS al track actual si la grabación está activa, actualiza el mapa y la interfaz.
        /// </summary>
        /// <param name="lastPosition">Última posición GPS obtenida.</param>
        public override async Task AddPosition(Position lastPosition)
        {
            try
            {
                if (IsListening)
                {
                    //Crea una nueva instancia de Position con los datos GPS
                    Position position = new()
                    {
                        Guid = Guid.NewGuid().ToString(),
                        Latitude = lastPosition.Latitude,
                        Longitude = lastPosition.Longitude,
                        Accuracy = lastPosition.Accuracy,
                        Altitude = lastPosition.Altitude,
                        Course = lastPosition.Course,
                        Timestamp = lastPosition.Timestamp,
                    };

                    //Crea una asociación con el Track
                    TrackPoint trackPoint = new()
                    {
                        Guid = Guid.NewGuid().ToString(),
                        TrackGuid = CurrentTrack.Guid,
                        PositionGuid = position.Guid,
                        Position = position,
                    };

                    //Añade el punto al Track actual
                    CurrentTrack.TrackPoints.Add(trackPoint);

                    //Actualiza el total de puntos
                    Context.LbTotalPointsPublic.Text = $"Puntos: {CurrentTrack.TrackPoints.Count}";

                    //Borra el dibujo anterior
                    Context.ExecuteJavaScript(_trackScriptBuilder.GetClearTracks());

                    if (LastPosition != null && CurrentTrack.TotalPoints >= 3 && configuration.KalmanFilterEnabled)
                    {
                        var interval = (position.Timestamp - LastPosition.Timestamp).TotalSeconds;

                        _gpsPathSmoother.UpdateParameters(interval, position.Accuracy);

                        List<Position> positions = _gpsPathSmoother.SmoothBatch(CurrentTrack.TrackPoints.Select(a => a.Position).ToList());

                        CurrentTrack.TrackPoints.Clear();
                        foreach (var pos in positions)
                        {
                            pos.Guid = Guid.NewGuid().ToString();
                            TrackPoint smoothedTrackPoint = new()
                            {
                                Guid = Guid.NewGuid().ToString(),
                                TrackGuid = CurrentTrack.Guid,
                                PositionGuid = pos.Guid,
                                Position = pos,
                            };
                            CurrentTrack.TrackPoints.Add(smoothedTrackPoint);
                        }
                    }

                    //Pinta el Track en el mapa
                    Context.ExecuteJavaScript(_trackScriptBuilder.GetTrack(CurrentTrack));

                    LastPosition = position;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptionManager.HandleException(ex, this.Context);
            }

            await Task.CompletedTask;
        }
    }
}