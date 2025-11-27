using System.Reflection;

namespace WayPrecision
{
    public partial class App : Application
    {
        public IServiceProvider Services { get; }
        public static string AppVersion => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

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