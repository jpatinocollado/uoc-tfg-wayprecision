using System.Collections.ObjectModel;
using WayPrecision.Domain.Models;

namespace WayPrecision;

public partial class WaypointsPage : ContentPage
{
    //public ObservableCollection<Waypoint> data { get; set; } = new();

    public WaypointsPage()
    {
        InitializeComponent();

        //BindingContext = this;
        LoadWaypoints();
    }

    private void LoadWaypoints()
    {
    }

    private async void OnEditWaypointClicked(object sender, EventArgs e)
    {
        //if (sender is Button button && button.CommandParameter is object waypoint)
        //{
        //    //await Navigation.PushAsync(new WaypointDetailPage(waypoint));

        //    LoadWaypoints();
        //}
    }

    private async void OnViewOnMapClicked(object sender, EventArgs e)
    {
        //if (sender is Button btn && btn.CommandParameter is Waypoint waypoint)
        //{
        //    // TODO: Navega o muestra el waypoint en el mapa
        //    await DisplayAlert("Mapa", $"Visualizar {waypoint.Name} en el mapa", "OK");
        //}
    }
}