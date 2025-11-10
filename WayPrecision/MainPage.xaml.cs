using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using WayPrecision.Domain.Data;
using WayPrecision.Domain.Map.Scripting;
using WayPrecision.Domain.Models;
using WayPrecision.Domain.Pages;
using WayPrecision.Domain.Sensors.Location;
using WayPrecision.Domain.Services;

namespace WayPrecision
{
    public partial class MainPage : ContentPage, IQueryAttributable
    {
        private readonly IService<Track> _trackService;
        private readonly WaypointService _waypointService;

        private readonly IGpsManager gpsManager;
        private GpsLocation? _lastPosition = null;

        private bool isWebViewReady = false;
        private bool _locationEnable = false;
        private bool _locationCenterEnable = false;

        private string? _pendingWaypointGuid;
        private string? _pendingTrackGuid;
        private bool _isAppeared;

        public MainPage(WaypointService waypointService, IService<Track> trackService)
        {
            InitializeComponent();

            _waypointService = waypointService;
            _trackService = trackService;

            MapWebView.Navigated += OnMapWebViewNavigated;
            MapWebView.Loaded += OnMapWebView_Loaded;

            LoadOnlineOpenStreetMaps();

            gpsManager = new InternalGpsManager();
            gpsManager.PositionChanged += OnPositionChanged;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            //Navigate to waypoint if guid is provided
            if (query.TryGetValue("waypointGuid", out var guidWaypointObj) && guidWaypointObj is string guidWaypoint)
            {
                _pendingWaypointGuid = guidWaypoint;
            }
            else if (query.TryGetValue("trackGuid", out var guidTrackObj) && guidTrackObj is string guidTrack)
            {
                _pendingTrackGuid = guidTrack;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (isWebViewReady)
            {
                PaintElements();

                _isAppeared = true;
                TryFitElement();
            }
        }

        private void OnMapWebView_Loaded(object? sender, EventArgs e)
        {
            isWebViewReady = true;
        }

        private void OnMapWebViewNavigated(object? sender, WebNavigatedEventArgs e)
        {
            isWebViewReady = true;
        }

        private void OnPositionChanged(object? sender, LocationEventArgs e)
        {
            _lastPosition = e.Location;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                UpdatePanelDadesGps();
                await UpdateMapLocation();
            });
        }

        private void OnShowLocationClicked(object sender, EventArgs e)
        {
            // Lógica para mostrar los datos de ubicación
            pnDades.IsVisible = !pnDades.IsVisible;
        }

        private void OnCreateWaypointClicked(object sender, EventArgs e)
        {
            // Lógica para crear un waypoint
            if (_locationEnable && _lastPosition != null)
            {
                DateTime dateTime = DateTime.UtcNow;

                Waypoint waypoint = new Waypoint
                {
                    Name = "",
                    Observation = "",
                    Created = dateTime.ToString("o"),
                    Position = new Position
                    {
                        Latitude = _lastPosition.Latitude,
                        Longitude = _lastPosition.Longitude,
                        Accuracy = _lastPosition.Accuracy,
                        Altitude = _lastPosition.Altitude,
                        Course = _lastPosition.Course,
                        Timestamp = dateTime.ToString("o"),
                    }
                };

                Navigation.PushAsync(new WaypointDetailPage(waypoint, DetailPageMode.Created));
            }
            else
                DisplayAlert("Crear Waypoint", "La ubicación no está habilitada o no se ha obtenido una ubicación válida.", "Aceptar");
        }

        private void OnCreateTrackClicked(object sender, EventArgs e)
        {
            // Lógica para crear un track
            if (_locationEnable && _lastPosition != null)
            {
            }
            else
                DisplayAlert("Crear Track", "La ubicación no está habilitada o no se ha obtenido una ubicación válida.", "Aceptar");
        }

        private void LoadOnlineOpenStreetMaps()
        {
            var html = string.Empty;
            using (var stream = FileSystem.OpenAppPackageFileAsync("map.html").Result)
            using (var reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();
            }

            var htmlSource = new HtmlWebViewSource
            {
                Html = html
            };

            MapWebView.Source = htmlSource;
        }

        private void UpdatePanelDadesGps()
        {
            lbLatitud.Text = $"Lat: -";
            lbLongitud.Text = $"Lng: -";
            lbPrecision.Text = $"Precisión: -";

            if (_locationEnable && _lastPosition != null)
            {
                CultureInfo ct = CultureInfo.InvariantCulture;
                double lat = Math.Round(_lastPosition.Latitude, 6);
                double lng = Math.Round(_lastPosition.Longitude, 6);
                double? acc = _lastPosition.Accuracy;

                lbLatitud.Text = $"Lat: {lat.ToString(ct)}";
                lbLongitud.Text = $"Lng: {lng.ToString(ct)}";

                if (acc.HasValue)
                    lbPrecision.Text = $"Precisión: {Math.Round(acc.Value, 2).ToString(ct)} m";
            }
        }

        private async Task UpdateMapLocation()
        {
            if (_lastPosition == null || !isWebViewReady || !_locationEnable)
                return;

            try
            {
                string direction = "undefined";

                if (_lastPosition.Course.HasValue)
                    direction = _lastPosition.Course.Value.ToString(CultureInfo.InvariantCulture);

                string js = $"updatePosition({_lastPosition.Latitude.ToString(CultureInfo.InvariantCulture)}, {_lastPosition.Longitude.ToString(CultureInfo.InvariantCulture)}, {_locationCenterEnable.ToString().ToLower()}, {direction});";
                await MapWebView.EvaluateJavaScriptAsync(js);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error actualizando ubicación: {ex.Message}");
            }
        }

        public void SetEnableLocation(bool value)
        {
            _locationEnable = value;

            if (value)
                _ = gpsManager.StartListeningAsync();
            else
                _ = gpsManager.StopListeningAsync();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdatePanelDadesGps();
            });
        }

        public void SetEnableCenterLocation(bool value)
        {
            _locationCenterEnable = value;
        }

        public void SetZoom(string zoom)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                lbZoom.Text = $"Zoom: {(!string.IsNullOrWhiteSpace(zoom) ? zoom : "-")}";
            });
        }

        #region Elements

        public void CreateWaypoint(double lat, double lng)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DateTime dateTime = DateTime.UtcNow;

                Waypoint waypoint = new Waypoint
                {
                    Name = "",
                    Observation = "",
                    Created = dateTime.ToString("o"),
                    Position = new Position
                    {
                        Latitude = lat,
                        Longitude = lng,
                        Timestamp = dateTime.ToString("o"),
                    }
                };

                Navigation.PushAsync(new WaypointDetailPage(waypoint, DetailPageMode.Created));
            });
        }

        public void EditWaypoint(string id)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                Waypoint waypoint = await _waypointService.GetByIdAsync(id);
                await Navigation.PushAsync(new WaypointDetailPage(waypoint, DetailPageMode.Edited));
            });
        }

        //public void CreateTracks()
        //{
        //    MainThread.BeginInvokeOnMainThread(() =>
        //    {
        //        //DateTime dateTime = DateTime.UtcNow;

        //        //Waypoint waypoint = new Waypoint
        //        //{
        //        //    Name = "",
        //        //    Observation = "",
        //        //    Created = dateTime.ToString("o"),
        //        //    Position = new Position
        //        //    {
        //        //        Latitude = lat,
        //        //        Longitude = lng,
        //        //        Timestamp = dateTime.ToString("o"),
        //        //    }
        //        //};

        //        //Navigation.PushAsync(new WaypointDetailPage(waypoint, DetailPageMode.Created));
        //    });
        //}

        public void EditTrack(string id)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                Track track = await _trackService.GetByIdAsync(id);
                await Navigation.PushAsync(new TrackDetailPage(track, DetailPageMode.Edited));
            });
        }

        private async Task TryFitElement()
        {
            if (_isAppeared && !string.IsNullOrEmpty(_pendingTrackGuid))
            {
                TrackScriptBuilder script = new TrackScriptBuilder();
                string js = script.FitTrack(_pendingTrackGuid).Render();
                await MapWebView.EvaluateJavaScriptAsync(js);
                _pendingTrackGuid = null; // Solo una vez
            }

            if (_isAppeared && !string.IsNullOrEmpty(_pendingWaypointGuid))
            {
                WaypointScriptBuilder script = new WaypointScriptBuilder();
                string js = script.FitWaypoint(_pendingWaypointGuid).Render();
                await MapWebView.EvaluateJavaScriptAsync(js);
                _pendingWaypointGuid = null; // Solo una vez
            }
        }

        public void PaintElements()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var waypoints = await _waypointService.GetAllAsync();
                var tracks = await _trackService.GetAllAsync();

                TrackScriptBuilder scTracks = new TrackScriptBuilder();
                WaypointScriptBuilder scWaypoints = new WaypointScriptBuilder();

                await MapWebView.EvaluateJavaScriptAsync(scWaypoints.GetClearWaypoints());
                foreach (var waypoint in waypoints)
                {
                    await MapWebView.EvaluateJavaScriptAsync(scWaypoints.GetWaypoint(waypoint));
                }

                await MapWebView.EvaluateJavaScriptAsync(scTracks.GetClearTracks());
                foreach (var track in tracks)
                {
                    await MapWebView.EvaluateJavaScriptAsync(scTracks.GetTrack(track));
                }
            });
        }

        #endregion Elements
    }
}