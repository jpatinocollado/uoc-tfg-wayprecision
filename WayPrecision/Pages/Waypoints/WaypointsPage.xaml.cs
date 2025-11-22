using System.Collections.ObjectModel;
using WayPrecision.Domain.Models;
using WayPrecision.Domain.Pages;
using WayPrecision.Domain.Services;

namespace WayPrecision;

public partial class WaypointsPage : ContentPage
{
    private readonly IService<Waypoint> _service;
    public ObservableCollection<Waypoint> Waypoints { get; set; } = [];

    public WaypointsPage(IService<Waypoint> service)
    {
        InitializeComponent();

        _service = service;

        BindingContext = this;

        _ = LoadWaypoints();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _ = LoadWaypoints();
    }

    private async Task LoadWaypoints()
    {
        var waypoints = await _service.GetAllAsync();
        Waypoints.Clear();
        foreach (var waypoint in waypoints)
            Waypoints.Add(waypoint);

        Title = $"Waypoints ({waypoints.Count})";
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Waypoint waypoint)
        {
            await Navigation.PushAsync(new WaypointDetailPage(waypoint, DetailPageMode.Edited));
        }
    }

    private async void OnViewOnMapClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Waypoint waypoint)
        {
            if (!waypoint.IsVisible)
                OnEyeClicked(sender, new EventArgs());

            //navega a la página del mapa y muestra el waypoint
            await Shell.Current.GoToAsync($"//MainPage?waypointGuid={waypoint.Guid}");
        }
    }

    private async void OnEyeClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Waypoint waypoint)
        {
            waypoint.IsVisible = !waypoint.IsVisible;
            await _service.UpdateAsync(waypoint);
        }
    }
}