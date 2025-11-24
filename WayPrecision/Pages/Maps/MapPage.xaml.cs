using Microsoft.Maui.Devices.Sensors;
using System.Globalization;
using WayPrecision.Domain.Helpers.Colors;
using WayPrecision.Domain.Helpers.Gps;
using WayPrecision.Domain.Helpers.Gps.Outliers;
using WayPrecision.Domain.Helpers.Gps.Smoothing;
using WayPrecision.Domain.Map.Scripting;
using WayPrecision.Domain.Models;
using WayPrecision.Domain.Pages;
using WayPrecision.Domain.Services;
using WayPrecision.Domain.Services.Configuracion;
using WayPrecision.Domain.Services.Location;
using WayPrecision.Pages.Maps;

namespace WayPrecision
{
    public partial class MainPage : ContentPage, IQueryAttributable
    {
        private readonly IConfigurationService _configurationService;
        private readonly IService<Track> _trackService;
        private readonly IService<Waypoint> _waypointService;

        private readonly IGpsManager _gpsManager;
        internal Position? _currentPosition = null;
        internal Position? _previusPosition = null;

        internal MapState State;
        internal bool isWebViewReady => (isWebViewNavigated && isWebViewLoaded);
        internal bool isWebViewNavigated = false;
        internal bool isWebViewLoaded = false;
        internal bool _locationEnable = false;
        internal bool _locationCenterEnable = false;

        internal string? _pendingWaypointGuid;
        internal string? _pendingTrackGuid;
        internal bool _isAppeared;

        private bool _firstLoadExecuted = false;

        public WebView MapWebViewPublic => MapWebView;

        public HorizontalStackLayout BtnStackLayoutDefaultPublic => BtnStackLayoutDefault;
        public VerticalStackLayout BtnStackLayoutTrackingPublic => BtnStackLayoutTracking;

        public Button BtnPlayPublic => btnPlay;
        public Button BtnPausePublic => btnPause;
        public Button BtnStopPublic => btnStop;
        public Button BtnCancelPublic => btnCancel;

        public Label LbTotalPointsPublic => lbTotalPoints;

        public Button BtnGpsDataPublic => btnGpsData;
        public Button BtnCreateWaypointPublic => btnCreateWaypoint;
        public Button BtnCreateTrackPublic => btnCreateTrack;

        public Border PnGpsDataPublic => pnGpsData;

        public MainPage(IService<Waypoint> waypointService, IService<Track> trackService, IConfigurationService configurationService, IGpsManager gpsManager)
        {
            InitializeComponent();

            //set services
            _waypointService = waypointService;
            _trackService = trackService;
            _configurationService = configurationService;

            MapWebView.Navigated += OnMapWebViewNavigated;
            MapWebView.Loaded += OnMapWebViewLoaded;

            // Load initial map
            LoadOnlineOpenStreetMaps();

            //GPS Management
            _gpsManager = gpsManager;
            _gpsManager.PositionChanged += OnPositionChanged;

            // Subscribe connectivity changes
            Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;

            //Transition to initial state
            TransitionTo(new MapStateDefault(_trackService, _configurationService));

            // Check initial connectivity
            CheckInitialConnectivity();
        }

        #region MAP INITIALIZATION

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

        #endregion MAP INITIALIZATION

        #region CONNECTIVITY

        private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            bool hasInternet = e.NetworkAccess == NetworkAccess.Internet;

            // Optionally you can verify remote reachability here
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (hasInternet)
                {
                    // Double check remote reachability to avoid captive portal issues
                    var reachable = await IsRemoteReachableAsync();
                    UpdateConnectionUi(reachable);
                }
                else
                {
                    UpdateConnectionUi(false);
                }
            });
        }

        private void CheckInitialConnectivity()
        {
            bool hasInternet = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

            if (hasInternet)
            {
                // Fire and forget remote check
                _ = Task.Run(async () =>
                {
                    var reachable = await IsRemoteReachableAsync();
                    MainThread.BeginInvokeOnMainThread(() => UpdateConnectionUi(reachable));
                });
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() => UpdateConnectionUi(false));
            }
        }

        private void UpdateConnectionUi(bool hasInternet)
        {
            // Access the banner via FindByName to avoid relying on generated backing field during compile-time analysis
            var banner = this.FindByName<Border>("pnNoInternet");
            if (banner != null)
                banner.IsVisible = !hasInternet;

            // If there's no internet, avoid loading remote resources
            if (!hasInternet)
            {
                // You might want to stop reloading the webview
                // and notify the user
                // Optionally show alert once
                // _ = DisplayAlert("Sin conexión", "No hay conexión a Internet. Algunas funcionalidades pueden no funcionar.", "OK");
            }
            else
            {
                if (!_firstLoadExecuted && isWebViewReady && isWebViewNavigated)
                {
                    MapWebView.Reload();
                }
            }
        }

        private async Task<bool> IsRemoteReachableAsync(string url = "https://www.google.com/generate_204")
        {
            try
            {
                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(4) };
                var res = await http.GetAsync(url);
                return res.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        #endregion CONNECTIVITY

        #region MAP EVENTS

        private void OnMapWebViewLoaded(object? sender, EventArgs e)
        {
            isWebViewLoaded = true;
            //isWebViewReady = true;
        }

        private void OnMapWebViewNavigated(object? sender, WebNavigatedEventArgs e)
        {
            ///*isWebViewReady*/ = true;
            isWebViewNavigated = true;
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

        public void SetZoom(string zoom)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                lbZoom.Text = $"Zoom: {(!string.IsNullOrWhiteSpace(zoom) ? zoom : "-")}";
            });
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

        public void SetFirstLoadExecuted()
        {
            _firstLoadExecuted = true;
        }

        #endregion MAP EVENTS

        #region GPS MANAGEMENT

        private void OnPositionChanged(object? sender, LocationEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                //Assign current position
                _currentPosition = e.Position;

                //Aplicamos el filtro de Outliers
                Configuration configuration = await _configurationService.GetOrCreateAsync();
                GpsParameters gpsParameters = new GpsParameters
                {
                    OutliersEnabled = configuration.OutliersFilterEnabled,
                    MinAccuracyMeters = configuration.GpsAccuracy,
                };
                IGpsFilter outliersFilter = new OutliersFilter(gpsParameters);

                //update GPS data panel
                UpdatePanelDadesGps(_currentPosition);

                //update map location
                await UpdateMapLocation(_currentPosition);

                // si la posición no es un outlier la procesamos
                if (!outliersFilter.IsInvalid(_previusPosition, _currentPosition))
                {
                    //add position to current state
                    await State.AddPosition(_currentPosition);

                    //Assign previous position
                    _previusPosition = e.Position;
                }
            });
        }

        private void UpdatePanelDadesGps(Position? gpsLocation)
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

        private async Task UpdateMapLocation(Position? gpsLocation)
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
                string js = $"MapGpsManagerService.SetGpsPosition({lat}, {lng}, {center}, {direction});";
                ExecuteJavaScript(js);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error actualizando ubicación: {ex.Message}");
            }

            await Task.CompletedTask;
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

        #endregion GPS MANAGEMENT

        #region MAP ELEMENTS MANAGEMENT

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
                        Timestamp = dateTime
                    }
                };

                Navigation.PushAsync(new WaypointDetailPage(waypoint, DetailPageMode.Created));
            });
        }

        public void EditWaypoint(string id)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                Waypoint? waypoint = await _waypointService.GetByIdAsync(id);
                if (waypoint == null)
                    return;

                await Navigation.PushAsync(new WaypointDetailPage(waypoint, DetailPageMode.Edited));
            });
        }

        public void EditTrack(string id)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                Track? track = await _trackService.GetByIdAsync(id);
                if (track == null)
                    return;

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

            await Task.CompletedTask;
        }

        public void PaintElements()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (!isWebViewReady)
                    return;

                var waypoints = await _waypointService.GetAllAsync();
                var tracks = await _trackService.GetAllAsync();
                var configuration = await _configurationService.GetOrCreateAsync();

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
                    GpsParameters gpsParameters = new GpsParameters
                    {
                        KalmanEnabled = configuration.KalmanFilterEnabled,
                        MovingAverageEnabled = configuration.MovingAverageFilterEnabled,
                        OutliersEnabled = configuration.OutliersFilterEnabled,
                        MinAccuracyMeters = configuration.GpsAccuracy,
                    };
                    var smoother = new GpsPathSmoother(gpsParameters);
                    List<Position> positions = smoother.SmoothBatch(track.TrackPoints.Select(a => a.Position).ToList());

                    track.TrackPoints.Clear();
                    foreach (var pos in positions)
                    {
                        pos.Guid = Guid.NewGuid().ToString();
                        TrackPoint smoothedTrackPoint = new()
                        {
                            Guid = Guid.NewGuid().ToString(),
                            TrackGuid = track.Guid,
                            PositionGuid = pos.Guid,
                            Position = pos,
                        };
                        track.TrackPoints.Add(smoothedTrackPoint);
                    }

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
                Track? track = await _trackService.GetByIdAsync(idUpdatedTrack);

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

        #endregion MAP ELEMENTS MANAGEMENT

        public void TransitionTo(MapState state)
        {
            State?.Dispose();

            State = state;
            State.SetContext(this);
            State.Init();
        }
    }
}