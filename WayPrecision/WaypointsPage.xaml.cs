using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using WayPrecision.Domain.Data.UnitOfWork;
using WayPrecision.Domain.Models;
using WayPrecision.Domain.Services;

namespace WayPrecision;

public partial class WaypointsPage : ContentPage
{
    private readonly WaypointService service;
    public ObservableCollection<Waypoint> Waypoints { get; set; } = [];

    public WaypointsPage(IUnitOfWork unitOfWork)
    {
        InitializeComponent();

        service = new WaypointService(unitOfWork);

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
        var waypoints = await service.GetAllAsync();
        Waypoints.Clear();
        foreach (var waypoint in waypoints)
            Waypoints.Add(waypoint);

        Title = $"Waypoints ({waypoints.Count})";
    }

    private async void OnEditWaypointClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Waypoint waypoint)
        {
            await Navigation.PushAsync(new WaypointDetailPage(waypoint, WaypointDetailPageMode.Edited));
        }
    }

    private async void OnViewOnMapClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Waypoint waypoint)
        {
            // TODO: Navega o muestra el waypoint en el mapa
            await DisplayAlert("Mapa", $"Visualizar {waypoint.Name} en el mapa", "OK");
        }
    }
}