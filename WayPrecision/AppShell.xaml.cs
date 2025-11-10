using WayPrecision.Domain.Data.UnitOfWork;
using WayPrecision.Domain.Services;

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

        private async void OnSettingsPageClicked(object sender, EventArgs e)
        {
            if (App.Current == null)
                return;

            IConfigurationService config = ((App)Application.Current).Services.GetRequiredService<IConfigurationService>();
            await Shell.Current.Navigation.PushAsync(new SettingsPage(config));
        }

        private async void OnLicenseClicked(object sender, EventArgs e)
        {
            Uri uri = new Uri("https://github.com/jpatinocollado/uoc-tfg-wayprecision/blob/main/LICENSE.md");
            await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }
    }
}