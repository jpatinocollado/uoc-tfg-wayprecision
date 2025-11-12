using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using WayPrecision.Domain.Data;
using WayPrecision.Domain.Map.Scripting;
using WayPrecision.Domain.Models;
using WayPrecision.Domain.Pages;
using WayPrecision.Domain.Sensors.Location;
using WayPrecision.Domain.Services;
using WayPrecision.Pages.Maps;

namespace WayPrecision
{
    public partial class MainPage : ContentPage, IQueryAttributable
    {
        private readonly IConfigurationService _configurationService;
        private readonly IService<Track> _trackService;
        private readonly IService<Waypoint> _waypointService;

        private readonly IGpsManager _gpsManager;
        internal GpsLocation? _lastPosition = null;

        internal MapState State = null;
        internal bool isWebViewReady = false;
        internal bool _locationEnable = false;
        internal bool _locationCenterEnable = false;

        internal string? _pendingWaypointGuid;
        internal string? _pendingTrackGuid;
        internal bool _isAppeared;

        public WebView MapWebViewPublic => MapWebView;

        public HorizontalStackLayout BtnStackLayoutDefaultPublic => BtnStackLayoutDefault;
        public VerticalStackLayout BtnStackLayoutTrackingPublic => BtnStackLayoutTracking;

        public Button btnPlayPublic => btnPlay;
        public Button btnPausePublic => btnPause;
        public Button btnStopPublic => btnStop;
        public Button btnCancelPublic => btnCancel;

        public Label lbTotalPointsPublic => lbTotalPoints;

        public Button btnGpsDataPublic => btnGpsData;
        public Button btnCreateWaypointPublic => btnCreateWaypoint;
        public Button btnCreateTrackPublic => btnCreateTrack;

        public Frame pnGpsDataPublic => pnGpsData;

        public MainPage(IService<Waypoint> waypointService, IService<Track> trackService, IConfigurationService configurationService, IGpsManager gpsManager)
        {
            InitializeComponent();

            _waypointService = waypointService;
            _trackService = trackService;
            _configurationService = configurationService;

            MapWebView.Navigated += OnMapWebViewNavigated;
            MapWebView.Loaded += OnMapWebViewLoaded;
            MapWebView.Navigating += MapWebViewNavigating;

            LoadOnlineOpenStreetMaps();

            _gpsManager = gpsManager;
            _gpsManager.PositionChanged += OnPositionChanged;

            TransitionTo(new MapStateDefault(_trackService));
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            //Navigate to waypoint if guid is provided
            if (query.TryGetValue("waypointGuid", out var guidWaypointObj) && guidWaypointObj is string guidWaypoint)
            {
                _pendingWaypointGuid = guidWaypoint;
                Shell.Current.GoToAsync("//MainPage");
            }
            else if (query.TryGetValue("trackGuid", out var guidTrackObj) && guidTrackObj is string guidTrack)
            {
                _pendingTrackGuid = guidTrack;
                Shell.Current.GoToAsync("//MainPage");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (isWebViewReady)
            {
                PaintElements();

                _isAppeared = true;
                await TryFitElement();
            }

            Configuration configuration = await _configurationService.GetOrCreateAsync();
            await _gpsManager.ChangeGpsInterval(new TimeSpan(0, 0, configuration.GpsInterval));
        }

        private void MapWebViewNavigating(object? sender, WebNavigatingEventArgs e)
        {
            e.Cancel = true;
        }

        private void OnMapWebViewLoaded(object? sender, EventArgs e)
        {
            isWebViewReady = true;
        }

        private void OnMapWebViewNavigated(object? sender, WebNavigatedEventArgs e)
        {
            isWebViewReady = true;
        }

        public void ExecuteJavaScript(string script)
        {
            if (!isWebViewReady)
                return;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await MapWebView.EvaluateJavaScriptAsync(script);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error ejecutando JavaScript: {ex.Message}");
                }
            });
        }

        private void OnPositionChanged(object? sender, LocationEventArgs e)
        {
            //get last position
            _lastPosition = e.Location;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                //update GPS data panel
                UpdatePanelDadesGps(_lastPosition);

                //update map location
                await UpdateMapLocation(_lastPosition);

                //add position to current state
                State.AddPosition(_lastPosition);
            });
        }

        //private void OnCreateWaypointClicked(object sender, EventArgs e)
        //{
        //    // Lógica para crear un waypoint
        //    if (_locationEnable && _lastPosition != null)
        //    {
        //        DateTime dateTime = DateTime.UtcNow;

        //        Waypoint waypoint = new()
        //        {
        //            Name = "",
        //            Observation = "",
        //            Created = dateTime.ToString("o"),
        //            Position = new Position
        //            {
        //                Latitude = _lastPosition.Latitude,
        //                Longitude = _lastPosition.Longitude,
        //                Accuracy = _lastPosition.Accuracy,
        //                Altitude = _lastPosition.Altitude,
        //                Course = _lastPosition.Course,
        //                Timestamp = dateTime.ToString("o"),
        //            }
        //        };

        //        Navigation.PushAsync(new WaypointDetailPage(waypoint, DetailPageMode.Created));
        //    }
        //    else
        //        DisplayAlert("Crear Waypoint", "La ubicación no está habilitada o no se ha obtenido una ubicación válida.", "Aceptar");
        //}

        //private void OnCreateTrackClicked(object sender, EventArgs e)
        //{
        //    // Lógica para crear un track
        //    if (_locationEnable && _lastPosition != null)
        //    {
        //        TransitionTo(new MapStateTracking());
        //    }
        //    else
        //        DisplayAlert("Crear Track", "La ubicación no está habilitada o no se ha obtenido una ubicación válida.", "Aceptar");
        //}

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

        private void UpdatePanelDadesGps(GpsLocation? gpsLocation)
        {
            lbLatitud.Text = $"Lat: -";
            lbLongitud.Text = $"Lng: -";
            lbPrecision.Text = $"Precisión: -";

            if (_locationEnable && gpsLocation != null)
            {
                CultureInfo ct = CultureInfo.InvariantCulture;
                double lat = Math.Round(gpsLocation.Latitude, 6);
                double lng = Math.Round(gpsLocation.Longitude, 6);
                double? acc = gpsLocation.Accuracy;

                lbLatitud.Text = $"Lat: {lat.ToString(ct)}";
                lbLongitud.Text = $"Lng: {lng.ToString(ct)}";

                if (acc.HasValue)
                    lbPrecision.Text = $"Precisión: {Math.Round(acc.Value, 2).ToString(ct)} m";
            }
        }

        private async Task UpdateMapLocation(GpsLocation? gpsLocation)
        {
            if (gpsLocation == null || !isWebViewReady || !_locationEnable)
                return;

            try
            {
                string direction = "undefined";

                if (gpsLocation.Course.HasValue)
                    direction = gpsLocation.Course.Value.ToString(CultureInfo.InvariantCulture);

                string lat = gpsLocation.Latitude.ToString(CultureInfo.InvariantCulture);
                string lng = gpsLocation.Longitude.ToString(CultureInfo.InvariantCulture);
                string center = _locationCenterEnable.ToString().ToLower();
                string js = $"updatePosition({lat}, {lng}, {center}, {direction});";
                ExecuteJavaScript(js);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error actualizando ubicación: {ex.Message}");
            }
        }

        public async Task SetEnableLocation(bool value)
        {
            _locationEnable = value;

            if (value)
            {
                Configuration configuration = await _configurationService.GetOrCreateAsync();
                _ = _gpsManager.StartListeningAsync(new TimeSpan(0, 0, configuration.GpsInterval));
            }
            else
            {
                _ = _gpsManager.StopListeningAsync();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UpdatePanelDadesGps(null);
                });
            }
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

                Waypoint waypoint = new()
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
            if (!_isAppeared)
                return;

            if (!string.IsNullOrEmpty(_pendingTrackGuid))
            {
                TrackScriptBuilder script = new();
                ExecuteJavaScript(script.GetFitTrack(_pendingTrackGuid));
                _pendingTrackGuid = null; // Solo una vez
            }

            if (!string.IsNullOrEmpty(_pendingWaypointGuid))
            {
                WaypointScriptBuilder script = new();
                ExecuteJavaScript(script.GetFitWaypoint(_pendingWaypointGuid));
                _pendingWaypointGuid = null; // Solo una vez
            }
        }

        public void PaintElements()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (!isWebViewReady)
                    return;

                var waypoints = await _waypointService.GetAllAsync();
                var tracks = await _trackService.GetAllAsync();

                TrackScriptBuilder scTracks = new();
                WaypointScriptBuilder scWaypoints = new();

                ExecuteJavaScript(scWaypoints.GetClearWaypoints());
                foreach (var waypoint in waypoints)
                {
                    ExecuteJavaScript(scWaypoints.GetWaypoint(waypoint));
                }

                ExecuteJavaScript(scTracks.GetClearTracks());
                foreach (var track in tracks)
                {
                    ExecuteJavaScript(scTracks.GetTrack(track));
                }
            });
        }

        public void ClearElements()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (!isWebViewReady)
                    return;

                TrackScriptBuilder scTracks = new();
                WaypointScriptBuilder scWaypoints = new();

                ExecuteJavaScript(scWaypoints.GetClearWaypoints());
                ExecuteJavaScript(scTracks.GetClearTracks());
            });
        }

        internal async Task UpdateTrackDataGeometry(string idUpdatedTrack, double? trackLength, double? trackArea, double? trackPerimeter)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                Track track = await _trackService.GetByIdAsync(idUpdatedTrack);

                if (track == null)
                    return;

                if (track.IsOpened)
                    track.Length = trackLength;
                else
                {
                    track.Length = trackPerimeter;
                    track.Area = trackArea;
                }
                await _trackService.UpdateAsync(track);
            });

            await Task.CompletedTask;
        }

        #endregion Elements

        public void TransitionTo(MapState state)
        {
            if (State != null)
                State.Dispose();

            State = state;
            State.SetContext(this);
            State.Init();
        }

        internal async Task ShowLoading(string message)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                ExecuteJavaScript($"LoadingManagerService.Show('{message}');");
            });

            await Task.CompletedTask;
        }

        internal async Task HideLoading()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                ExecuteJavaScript("LoadingManagerService.Hide();");
            });

            await Task.CompletedTask;
        }
    }
}