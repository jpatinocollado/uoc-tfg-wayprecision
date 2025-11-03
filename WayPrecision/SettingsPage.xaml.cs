using Microsoft.VisualBasic.FileIO;
using WayPrecision.Domain.Data;
using WayPrecision.Domain.Models;
using WayPrecision.Domain.Services;

namespace WayPrecision;

public partial class SettingsPage : ContentPage
{
    internal class UnitOption
    {
        public string Key { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
    }

    private readonly ConfigurationService _configurationService;
    private Configuration _currentConfig;

    public SettingsPage(IUnitOfWork unitOfWork)
    {
        InitializeComponent();

        // Resolver IUnitOfWork desde el contenedor de dependencias
        //var unitOfWork = Application.Current.Handler.MauiContext.Services.GetRequiredService<IUnitOfWork>();
        _configurationService = new ConfigurationService(unitOfWork);

        // Cargar configuración al iniciar la página
        Loaded += async (s, e) => await LoadConfigurationAsync();

        // Guardar configuración al hacer clic en el botón
        SaveButton.Clicked += async (s, e) => await SaveConfigurationAsync();
    }

    private async Task LoadConfigurationAsync()
    {
        var areaOptions = new List<UnitOption>
    {
        new() { Key = UnitEnum.MetrosCuadrados.ToString(), Display = "m²" },
        new() { Key = UnitEnum.KilometrosCuadrados.ToString(), Display = "km²" },
        new() { Key = UnitEnum.Hectareas.ToString(), Display = "ha" }
    };
        var lengthOptions = new List<UnitOption>
    {
        new() { Key = UnitEnum.Metros.ToString(), Display = "m" },
        new() { Key = UnitEnum.Kilometros.ToString(), Display = "km" }
    };

        AreaUnitsPicker.ItemsSource = areaOptions;
        AreaUnitsPicker.ItemDisplayBinding = new Binding("Display");

        LengthUnitsPicker.ItemsSource = lengthOptions;
        LengthUnitsPicker.ItemDisplayBinding = new Binding("Display");

        _currentConfig = await _configurationService.GetOrCreateAsync();

        GpsIntervalEntry.Text = _currentConfig.GpsInterval.ToString();

        AreaUnitsPicker.SelectedIndex = areaOptions.FindIndex(x => x.Key == _currentConfig.AreaUnits);
        LengthUnitsPicker.SelectedIndex = lengthOptions.FindIndex(x => x.Key == _currentConfig.LengthUnits);
    }

    private async Task SaveConfigurationAsync()
    {
        if (_currentConfig == null)
            return;

        if (!int.TryParse(GpsIntervalEntry.Text, out int gpsInterval))
        {
            await DisplayAlert("Error", "El intervalo GPS debe ser un número entero válido.", "OK");
            return;
        }
        if (gpsInterval <= 0 || gpsInterval > 30)
        {
            await DisplayAlert("Error", "El intervalo GPS debe ser mayor que 0 y menor o igual a 30.", "OK");
            return;
        }
        _currentConfig.GpsInterval = gpsInterval;

        if (AreaUnitsPicker.SelectedIndex >= 0)
            _currentConfig.AreaUnits = ((UnitOption)AreaUnitsPicker.SelectedItem).Key;

        if (LengthUnitsPicker.SelectedIndex >= 0)
            _currentConfig.LengthUnits = ((UnitOption)LengthUnitsPicker.SelectedItem).Key;

        await _configurationService.SaveAsync(_currentConfig);

        await DisplayAlert("Configuración", "Configuración guardada correctamente.", "OK");
    }
}