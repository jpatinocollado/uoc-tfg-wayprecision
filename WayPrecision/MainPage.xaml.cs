namespace WayPrecision
{
    public partial class MainPage : ContentPage
    {
        private bool isWebViewReady = false;
        private bool _locationEnable = false;
        private bool _locationCenterEnable = false;

        public MainPage()
        {
            InitializeComponent();

            MapWebView.Navigated += OnMapWebViewNavigated;
            MapWebView.Loaded += OnMapWebView_Loaded;

            LoadOnlineOpenStreetMaps();
        }

        private void OnMapWebView_Loaded(object? sender, EventArgs e)
        {
            isWebViewReady = true;
        }

        private void OnMapWebViewNavigated(object? sender, WebNavigatedEventArgs e)
        {
            isWebViewReady = true;
        }

        private void OnShowLocationClicked(object sender, EventArgs e)
        {
            // Lógica para mostrar los datos de ubicación
            pnDades.IsVisible = !pnDades.IsVisible;
        }

        private void OnCreateWaypointClicked(object sender, EventArgs e)
        {
            //// Lógica para crear un waypoint
            //if (_locationEnable && lastLocation != null)
            //{
            //    string msg = $"Waypoint creado en Lat: {lastLocation.Latitude}, Lng: {lastLocation.Longitude}";
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
    }
}