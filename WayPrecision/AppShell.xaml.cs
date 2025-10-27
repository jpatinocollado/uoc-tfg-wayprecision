namespace WayPrecision
{
    public partial class AppShell : Shell
    {
        public Command OpenEmailCommand { get; }

        public AppShell()
        {
            InitializeComponent();

            OpenEmailCommand = new Command(async () =>
            {
                await Launcher.OpenAsync("mailto:jpcollado@uoc.edu");
            });

            BindingContext = this;
        }

        private static async void OnLinkedInTapped(object sender, EventArgs e)
        {
            var url = "https://www.linkedin.com/in/jesuspatinocollado/";
            await Launcher.Default.OpenAsync(url);
        }
    }
}