using System.Threading.Tasks;
using WayPrecision.Domain.Map.Scripting;
using WayPrecision.Domain.Models;
using WayPrecision.Domain.Sensors.Location;
using WayPrecision.Domain.Services;

namespace WayPrecision.Pages.Maps
{
    public class MapStateTracking : MapState
    {
        private readonly TrackScriptBuilder _trackScriptBuilder;
        private readonly IService<Track> _service;

        private Track CurrentTrack;
        private bool IsListening = false;

        public MapStateTracking(IService<Track> service)
        {
            _trackScriptBuilder = new TrackScriptBuilder();
            _service = service;
        }

        public override void Init()
        {
            //Initialize current track
            CurrentTrack = new()
            {
                Guid = Guid.NewGuid().ToString(),
                TypeGeometry = TypeGeometry.LineString,
                Created = DateTime.UtcNow.ToString("o"),
                TrackPoints = new List<TrackPoint>(),
                IsOpened = true
            };

            //label total puntos
            MapPage.lbTotalPointsPublic.Text = "Puntos: 0";

            //Ponemos visibles los botones del pie de pagina
            MapPage.BtnStackLayoutDefaultPublic.IsVisible = false;
            MapPage.BtnStackLayoutTrackingPublic.IsVisible = true;
            MapPage.pnGpsDataPublic.IsVisible = true;

            //Register buttons events
            MapPage.btnPlayPublic.Clicked += OnPlayClicked;
            MapPage.btnPausePublic.Clicked += OnPauseClicked;
            MapPage.btnStopPublic.Clicked += OnStopClicked;
            MapPage.btnCancelPublic.Clicked += OnCancelClicked;

            //State Play default
            MapPage.btnPlayPublic.IsEnabled = false;
            MapPage.btnPausePublic.IsEnabled = true;
            MapPage.btnStopPublic.IsEnabled = true;

            //Start listening GPS
            IsListening = true;

            //Clear elements
            MapPage.ClearElements();
        }

        public override void Close()
        {
            //Unregister buttons events
            MapPage.btnPlayPublic.Clicked -= OnPlayClicked;
            MapPage.btnPausePublic.Clicked -= OnPauseClicked;
            MapPage.btnStopPublic.Clicked -= OnStopClicked;
            MapPage.btnCancelPublic.Clicked -= OnCancelClicked;
        }

        private void OnPlayClicked(object? sender, EventArgs e)
        {
            //start listening GPS
            IsListening = true;

            //Update buttons state
            MapPage.btnPlayPublic.IsEnabled = false;
            MapPage.btnPausePublic.IsEnabled = true;
            MapPage.btnStopPublic.IsEnabled = true;
        }

        private void OnPauseClicked(object? sender, EventArgs e)
        {
            //stop listening GPS
            IsListening = false;

            //Update buttons state
            MapPage.btnPlayPublic.IsEnabled = true;
            MapPage.btnPausePublic.IsEnabled = false;
            MapPage.btnStopPublic.IsEnabled = true;
        }

        private async void OnStopClicked(object? sender, EventArgs e)
        {
            //al hacer stop, procedemos a finalizar el track, lo primero es ponerlo en pausa
            OnPauseClicked(null, new EventArgs());

            //Finalize current track
            CurrentTrack.Finalized = DateTime.UtcNow.ToString("o");

            //contrlamos si se cierra el track
            bool cerrarTrack = false;
            if (CurrentTrack.TrackPoints.Count == 0)
            {
                //Display alert if there are no track points
                await MapPage.DisplayAlert(
                    "Finalizar Track",
                    "El track no tiene puntos y se cerrará automáticamente.",
                    "Aceptar"
                );

                MapPage.TransitionTo(new MapStateDefault(_service));
            }
            else if (CurrentTrack.TrackPoints.Count <= 2)
            {
                //Display alert, el track se creará como una linea
                await MapPage.DisplayAlert(
                     "Finalizar Track",
                     "El track tiene 2 o menos puntos y se creará como una línea.",
                     "Aceptar"
                 );
            }
            else
            {
                // Pregunta al usuario si quiere cerrar el track
                cerrarTrack = await MapPage.DisplayAlert(
                    "Finalizar Track",
                    "¿Quieres cerrar el track?",
                    "Sí",
                    "No"
                );
            }

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
                nameTrack = await MapPage.DisplayPromptAsync("Nombre del Track", "Introduce el nombre del track:", accept: "Aceptar", cancel: "Cancelar", maxLength: 50);

            CurrentTrack.Name = nameTrack;
            CurrentTrack = await _service.CreateAsync(CurrentTrack);

            await MapPage.ShowLoading("Calculando <br/> geometrías...");

            MapPage.TransitionTo(new MapStateDefault(_service));

            // espera 10 segundos para que se calculen las medidas
            await Task.Delay(4000);

            await MapPage.HideLoading();

            MapPage.EditTrack(CurrentTrack.Guid);
        }

        private async void OnCancelClicked(object? sender, EventArgs e)
        {
            bool reanudarTrack = IsListening;

            //al cancelar el track, lo primero es ponerlo en pausa
            OnPauseClicked(null, new EventArgs());

            //preguntamos si quiere cancelar el Track
            bool cancelarTrack = await MapPage.DisplayAlert(
                "Finalizar Track",
                "¿Quieres Cancelar el track?",
                "Sí",
                "No"
            );

            if (cancelarTrack)
            {
                //Hacemos la transición de estado sin guardar el track
                MapPage.TransitionTo(new MapStateDefault(_service));
            }
            else
            {
                //Si hay que reanudar el Track simulamos pulsar Pause
                if (reanudarTrack)
                    OnPlayClicked(null, new EventArgs());
            }

        }

        public override async Task AddPosition(GpsLocation lastPosition)
        {
            if (IsListening)
            {
                //Create new position from last GPS location
                Position position = new()
                {
                    Guid = Guid.NewGuid().ToString(),
                    Latitude = lastPosition.Latitude,
                    Longitude = lastPosition.Longitude,
                    Accuracy = lastPosition.Accuracy,
                    Altitude = lastPosition.Altitude,
                    Course = lastPosition.Course,
                    Timestamp = lastPosition.Timestamp.ToString("o"),
                };

                //Create new track point and add to current track
                TrackPoint trackPoint = new()
                {
                    Guid = Guid.NewGuid().ToString(),
                    TrackGuid = CurrentTrack.Guid,
                    PositionGuid = position.Guid,
                    Position = position,
                };

                //Add track point to current track
                CurrentTrack.TrackPoints.Add(trackPoint);

                //Update label total points
                MapPage.lbTotalPointsPublic.Text = $"Puntos: {CurrentTrack.TrackPoints.Count}";

                //Paint Track in map
                await MapPage.MapWebViewPublic.EvaluateJavaScriptAsync(_trackScriptBuilder.GetClearTracks());
                await MapPage.MapWebViewPublic.EvaluateJavaScriptAsync(_trackScriptBuilder.GetTrack(CurrentTrack));
            }

            await Task.CompletedTask;
        }
    }
}