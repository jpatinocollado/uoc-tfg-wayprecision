using WayPrecision.Domain.Exceptions;
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

        TrackingModePicker.SelectedIndexChanged += async (s, e) => await TrackModeChanged();
    }

    private async Task LoadConfigurationAsync()
    {
        try
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
            var trackkingModel = new List<UnitOption>
            {
                new (){ Key = TrackingModeEnum.GPS.ToString(), Display = "GPS" },
                new (){ Key = TrackingModeEnum.Manual.ToString(), Display = "Manual" }
            };

            AreaUnitsPicker.ItemsSource = areaOptions;
            AreaUnitsPicker.ItemDisplayBinding = new Binding("Display");

            LengthUnitsPicker.ItemsSource = lengthOptions;
            LengthUnitsPicker.ItemDisplayBinding = new Binding("Display");

            TrackingModePicker.ItemsSource = trackkingModel;
            TrackingModePicker.ItemDisplayBinding = new Binding("Display");

            _currentConfig = await _configurationService.GetOrCreateAsync();

            GpsIntervalEntry.Text = _currentConfig.GpsInterval.ToString();
            GpsAccuracyEntry.Text = _currentConfig.GpsAccuracy.ToString();

            AreaUnitsPicker.SelectedIndex = areaOptions.FindIndex(x => x.Key == _currentConfig.AreaUnits);
            LengthUnitsPicker.SelectedIndex = lengthOptions.FindIndex(x => x.Key == _currentConfig.LengthUnits);
            TrackingModePicker.SelectedIndex = trackkingModel.FindIndex(x => x.Key == _currentConfig.TrackingMode);

            OutliersSwitch.IsToggled = _currentConfig.OutliersFilterEnabled;
            KalmanSwitch.IsToggled = _currentConfig.KalmanFilterEnabled;
            MovingAvegareSwitch.IsToggled = _currentConfig.MovingAverageFilterEnabled;
        }
        catch (Exception ex)
        {
            GlobalExceptionManager.HandleException(ex, this);
        }
    }

    private async Task SaveConfigurationAsync()
    {
        try
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
            if (gpsAccuracy < 0 || gpsAccuracy > 20)
            {
                await DisplayAlert("Error", "La precisión mínima del GPS debe estar en el rango [0-20] metros.", "OK");
                return;
            }
            _currentConfig.GpsAccuracy = gpsAccuracy;

            // Guardar las unidades seleccionadas
            if (AreaUnitsPicker.SelectedIndex >= 0)
                _currentConfig.AreaUnits = ((UnitOption)AreaUnitsPicker.SelectedItem).Key;

            // Guardar las unidades seleccionadas
            if (LengthUnitsPicker.SelectedIndex >= 0)
                _currentConfig.LengthUnits = ((UnitOption)LengthUnitsPicker.SelectedItem).Key;

            // Guardar el modo de rastreo seleccionado
            if (TrackingModePicker.SelectedIndex >= 0)
                _currentConfig.TrackingMode = ((UnitOption)TrackingModePicker.SelectedItem).Key;

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
        catch (Exception ex)
        {
            GlobalExceptionManager.HandleException(ex, this);
        }
    }

    private async Task CancelAsync()
    {
        try
        {
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            GlobalExceptionManager.HandleException(ex, this);
        }
    }

    private async Task TrackModeChanged()
    {
        TrackingModeEnum trackMode = TrackingModeEnum.GPS;

        if (TrackingModePicker.SelectedIndex >= 0)
        {
            var key = ((UnitOption)TrackingModePicker.SelectedItem).Key;
            if (!Enum.TryParse<TrackingModeEnum>(key, ignoreCase: true, out var parsed))
                parsed = TrackingModeEnum.GPS;
            trackMode = parsed;
        }

        switch (trackMode)
        {
            case TrackingModeEnum.GPS:
                OutliersSwitch.IsToggled = true;
                KalmanSwitch.IsToggled = true;
                MovingAvegareSwitch.IsToggled = true;

                OutliersSwitch.IsEnabled = true;
                KalmanSwitch.IsEnabled = true;
                MovingAvegareSwitch.IsEnabled = true;
                break;

            case TrackingModeEnum.Manual:
                OutliersSwitch.IsToggled = false;
                KalmanSwitch.IsToggled = false;
                MovingAvegareSwitch.IsToggled = false;

                OutliersSwitch.IsEnabled = false;
                KalmanSwitch.IsEnabled = false;
                MovingAvegareSwitch.IsEnabled = false;
                break;

            default:
                break;
        }
    }
}