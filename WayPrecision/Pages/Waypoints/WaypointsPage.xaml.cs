using System.Collections.ObjectModel;
using WayPrecision.Domain.Exceptions;
using WayPrecision.Domain.Models;
using WayPrecision.Domain.Pages;
using WayPrecision.Domain.Services;
using WayPrecision.Pages.Waypoints;

namespace WayPrecision;

public partial class WaypointsPage : ContentPage
{
    private readonly IService<Waypoint> _service;
    public ObservableCollection<WaypointListItem> Waypoints { get; set; } = [];

    public WaypointsPage(IService<Waypoint> service)
    {
        InitializeComponent();

        _service = service;

        BindingContext = this;

        _ = LoadWaypoints();
    }

    protected override void OnAppearing()
    {
        try
        {
            base.OnAppearing();

            _ = LoadWaypoints();
        }
        catch (Exception ex)
        {
            GlobalExceptionManager.HandleException(ex, this);
        }
    }

    private async Task LoadWaypoints()
    {
        try
        {
            var waypoints = await _service.GetAllAsync();
            Waypoints.Clear();
            foreach (var waypoint in waypoints)
                Waypoints.Add(new WaypointListItem(waypoint));

            Title = $"Waypoints ({waypoints.Count})";
        }
        catch (Exception ex)
        {
            GlobalExceptionManager.HandleException(ex, this);
        }
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is Button button && button.CommandParameter is WaypointListItem item)
            {
                await Navigation.PushAsync(new WaypointDetailPage(item.Waypoint, DetailPageMode.Edited));
            }
        }
        catch (Exception ex)
        {
            GlobalExceptionManager.HandleException(ex, this);
        }
    }

    private async void OnViewOnMapClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is Button btn && btn.CommandParameter is WaypointListItem item)
            {
                if (!item.Waypoint.IsVisible)
                {
                    OnEyeClicked(sender, new EventArgs());

                    await Task.Delay(500);
                }

                //navega a la página del mapa y muestra el waypoint
                await Shell.Current.GoToAsync($"//MapPage?waypointGuid={item.Waypoint.Guid}");
            }
        }
        catch (Exception ex)
        {
            GlobalExceptionManager.HandleException(ex, this);
        }
    }

    private async void OnEyeClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is Button button && button.CommandParameter is WaypointListItem item)
            {
                item.IsVisible = !item.IsVisible;
                await _service.UpdateAsync(item.Waypoint);
            }
        }
        catch (Exception ex)
        {
            GlobalExceptionManager.HandleException(ex, this);
        }
    }
}