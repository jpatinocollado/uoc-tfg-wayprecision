using WayPrecision.Domain.Services;

namespace WayPrecision
{
    /// <summary>
    /// Representa la clase principal de la Shell de la aplicación.
    /// Gestiona la navegación y comandos globales.
    /// </summary>
    public partial class AppShell : Shell
    {
        /// <summary>
        /// URL del perfil de LinkedIn del autor.
        /// </summary>
        private const string LinkedInUrl = "https://www.linkedin.com/in/jesuspatinocollado/";

        /// <summary>
        /// URL de la licencia del proyecto.
        /// </summary>
        private const string LicenseUrl = "https://github.com/jpatinocollado/uoc-tfg-wayprecision/blob/main/LICENSE.md";

        /// <summary>
        /// Comando para abrir el cliente de correo electrónico.
        /// </summary>
        public Command OpenEmailCommand { get; }

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="AppShell"/>.
        /// </summary>
        public AppShell()
        {
            InitializeComponent();

            OpenEmailCommand = new Command(async () =>
            {
                await Launcher.OpenAsync("mailto:jpc_paty@hotmail.com");
            });

            BindingContext = this;
        }

        /// <summary>
        /// Evento que se ejecuta al pulsar el enlace de LinkedIn.
        /// </summary>
        /// <param name="sender">Objeto que genera el evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        private static async void OnLinkedInTapped(object sender, EventArgs e)
        {
            await Launcher.Default.OpenAsync(LinkedInUrl);
        }

        /// <summary>
        /// Evento que se ejecuta al pulsar la opción de configuración.
        /// </summary>
        /// <param name="sender">Objeto que genera el evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        private static async void OnSettingsPageClicked(object sender, EventArgs e)
        {
            if (App.Current == null)
                return;

            if (Shell.Current.Navigation.NavigationStack.LastOrDefault() is not SettingsPage)
            {
                IConfigurationService config = ((App)Application.Current).Services.GetRequiredService<IConfigurationService>();
                await Shell.Current.Navigation.PushAsync(new SettingsPage(config));

                Shell.Current.FlyoutIsPresented = false;
            }
        }

        /// <summary>
        /// Evento que se ejecuta al pulsar el enlace de la licencia.
        /// </summary>
        /// <param name="sender">Objeto que genera el evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        private static async void OnLicenseClicked(object sender, EventArgs e)
        {
            Uri uri = new Uri(LicenseUrl);
            await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }
    }
}