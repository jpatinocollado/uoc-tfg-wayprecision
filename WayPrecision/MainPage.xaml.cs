using System.Globalization;
using WayPrecision.Domain.Sensors.Location;

namespace WayPrecision
{
    public partial class MainPage : ContentPage
    {
        private readonly IGpsManager gpsManager;
        private Position? _lastPosition = null;

        private bool isWebViewReady = false;
        private bool _locationEnable = false;
        private bool _locationCenterEnable = false;

        public MainPage()
        {
            InitializeComponent();

            MapWebView.Navigated += OnMapWebViewNavigated;
            MapWebView.Loaded += OnMapWebView_Loaded;

            LoadOnlineOpenStreetMaps();

            gpsManager = new InternalGpsManager();
            gpsManager.PositionChanged += OnPositionChanged;
        }

        private void OnMapWebView_Loaded(object? sender, EventArgs e)
        {
            isWebViewReady = true;
        }

        private void OnMapWebViewNavigated(object? sender, WebNavigatedEventArgs e)
        {
            isWebViewReady = true;
        }

        private void OnPositionChanged(object? sender, PositionEventArgs e)
        {
            _lastPosition = e.Position;

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
            //// Lógica para crear un waypoint
            //if (_locationEnable && _lastPosition != null)
            //{
            //    string msg = $"Waypoint creado en Lat: {_lastPosition.Latitude}, Lng: {_lastPosition.Longitude}";
            //    DisplayAlert("Crear Waypoint", msg, "OK");
            //}
            //else
            //{
            //    DisplayAlert("Crear Waypoint", "La ubicación no está habilitada o no se ha obtenido una ubicación válida.", "Aceptar");
            //}
        }

        private void OnCreateTrackClicked(object sender, EventArgs e)
        {
            // Lógica para crear un track
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
    }
}