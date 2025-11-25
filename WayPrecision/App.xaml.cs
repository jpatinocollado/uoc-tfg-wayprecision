namespace WayPrecision
{
    public partial class App : Application
    {
        public IServiceProvider Services { get; }

        public App(IServiceProvider services)
        {
            InitializeComponent();

            Services = services;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}