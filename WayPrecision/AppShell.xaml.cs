namespace WayPrecision
{
    public partial class AppShell : Shell
    {
        public Command OpenEmailCommand { get; }

        public AppShell()
        {
            InitializeComponent();

            //Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));

            OpenEmailCommand = new Command(async () =>
            {
                await Launcher.OpenAsync("mailto:jpcollado@uoc.edu");
            });

            BindingContext = this;
        }

        private static async void OnLinkedInTapped(object sender, EventArgs e)
        {
            await Launcher.Default.OpenAsync("https://www.linkedin.com/in/jesuspatinocollado/");
        }
    }
}