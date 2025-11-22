using WayPrecision.Domain.Models;
using WayPrecision.Domain.Services.Configuracion;

namespace WayPrecision;

public partial class SettingsPage : ContentPage
{
    internal class UnitOption
    {
        public string Key { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
    }

    private readonly IConfigurationService _configurationService;
    private Configuration _currentConfig;

    public SettingsPage(IConfigurationService configurationService)
    {
        InitializeComponent();

        // Resolver IUnitOfWork desde el contenedor de dependencias
        _configurationService = configurationService;

        // Cargar configuración al iniciar la página
        Loaded += async (s, e) => await LoadConfigurationAsync();

        // Guardar configuración al hacer clic en el botón
        SaveButton.Clicked += async (s, e) => await SaveConfigurationAsync();

        // Cancelar y volver atrás al hacer clic en el botón
        CancelarButton.Clicked += async (s, e) => await CancelAsync();
    }

    private async Task LoadConfigurationAsync()
    {
        var areaOptions = new List<UnitOption>
    {
        new() { Key = UnitEnum.MetrosCuadrados.ToString(), Display = "Metros Cuadrados (m²)" },
        new() { Key = UnitEnum.KilometrosCuadrados.ToString(), Display = "Kilómetros Cuadrados (km²)" },
        new() { Key = UnitEnum.Hectareas.ToString(), Display = "Hectáreas (ha)" }
    };
        var lengthOptions = new List<UnitOption>
    {
        new() { Key = UnitEnum.Metros.ToString(), Display = "Metros (m)" },
        new() { Key = UnitEnum.Kilometros.ToString(), Display = "Kilómetros (km)" }
    };

        AreaUnitsPicker.ItemsSource = areaOptions;
        AreaUnitsPicker.ItemDisplayBinding = new Binding("Display");

        LengthUnitsPicker.ItemsSource = lengthOptions;
        LengthUnitsPicker.ItemDisplayBinding = new Binding("Display");

        _currentConfig = await _configurationService.GetOrCreateAsync();

        GpsIntervalEntry.Text = _currentConfig.GpsInterval.ToString();
        GpsAccuracyEntry.Text = _currentConfig.GpsAccuracy.ToString();

        AreaUnitsPicker.SelectedIndex = areaOptions.FindIndex(x => x.Key == _currentConfig.AreaUnits);
        LengthUnitsPicker.SelectedIndex = lengthOptions.FindIndex(x => x.Key == _currentConfig.LengthUnits);

        OutliersSwitch.IsToggled = _currentConfig.OutliersFilterEnabled;
        KalmanSwitch.IsToggled = _currentConfig.KalmanFilterEnabled;
        MovingAvegareSwitch.IsToggled = _currentConfig.MovingAverageFilterEnabled;
    }

    private async Task SaveConfigurationAsync()
    {
        if (_currentConfig == null)
            return;

        // Validar el intervalo GPS
        if (!int.TryParse(GpsIntervalEntry.Text, out int gpsInterval))
        {
            await DisplayAlert("Error", "El intervalo GPS debe ser un número entero válido.", "OK");
            return;
        }
        if (gpsInterval <= 0 || gpsInterval > 15)
        {
            await DisplayAlert("Error", "El intervalo GPS debe ser mayor que 0 y menor o igual a 15 segundos.", "OK");
            return;
        }
        _currentConfig.GpsInterval = gpsInterval;

        // Validar la precisión mínima
        if (!int.TryParse(GpsAccuracyEntry.Text, out int gpsAccuracy))
        {
            await DisplayAlert("Error", "La precisión mínima del GPS debe ser un número entero válido.", "OK");
            return;
        }
        if (gpsAccuracy <= 0 || gpsAccuracy > 20)
        {
            await DisplayAlert("Error", "La precisión mínima del GPS debe ser mayor que 0 y menor o igual a 20 segundos.", "OK");
            return;
        }
        _currentConfig.GpsAccuracy = gpsAccuracy;

        // Guardar las unidades seleccionadas
        if (AreaUnitsPicker.SelectedIndex >= 0)
            _currentConfig.AreaUnits = ((UnitOption)AreaUnitsPicker.SelectedItem).Key;

        // Guardar las unidades seleccionadas
        if (LengthUnitsPicker.SelectedIndex >= 0)
            _currentConfig.LengthUnits = ((UnitOption)LengthUnitsPicker.SelectedItem).Key;

        // Guardar el estado de los filtros de trazado
        _currentConfig.KalmanFilterEnabled = KalmanSwitch.IsToggled;
        _currentConfig.OutliersFilterEnabled = OutliersSwitch.IsToggled;
        _currentConfig.MovingAverageFilterEnabled = MovingAvegareSwitch.IsToggled;

        // Guardar la configuración
        await _configurationService.SaveAsync(_currentConfig);

        // Notificar al usuario
        await DisplayAlert("Configuración", "Configuración guardada correctamente.", "OK");

        // Volver a la página anterior
        await Navigation.PopAsync();
    }

    private async Task CancelAsync()
    {
        await Navigation.PopAsync();
    }
}