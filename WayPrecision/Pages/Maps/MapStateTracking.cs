using System.Threading.Tasks;
using WayPrecision.Domain.Models;
using WayPrecision.Domain.Sensors.Location;
using WayPrecision.Domain.Services;

namespace WayPrecision.Pages.Maps
{
    public class MapStateTracking : MapState
    {
        private readonly IService<Track> _service;

        private Track CurrentTrack = new();
        private bool IsListening = false;

        public MapStateTracking(IService<Track> service)
        {
            _service = service;
        }

        public override void Init()
        {
            //Initialize current track
            CurrentTrack.Guid = Guid.NewGuid().ToString();
            CurrentTrack.Created = DateTime.UtcNow.ToString("o");
            CurrentTrack.TrackPoints = new List<TrackPoint>();
            CurrentTrack.IsOpened = true;

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
            //stop listening GPS
            IsListening = false;

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

            await MapPage.ShowLoading("Calculando geometrías...");
            
            // espera 10 segundos para que se calculen las medidas
            await Task.Delay(10000);

            await MapPage.HideLoading();

            MapPage.TransitionTo(new MapStateDefault(_service));

            MapPage.EditTrack(CurrentTrack.Guid);
        }

        internal override void AddPosition(GpsLocation lastPosition)
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
            }
        }
    }
}