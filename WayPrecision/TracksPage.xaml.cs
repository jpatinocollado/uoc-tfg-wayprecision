using System.Collections.ObjectModel;
using WayPrecision.Domain.Data.UnitOfWork;
using WayPrecision.Domain.Models;
using WayPrecision.Domain.Services;

namespace WayPrecision;

public partial class TracksPage : ContentPage
{
    private readonly TrackService service;
    public ObservableCollection<Track> Tracks { get; set; } = [];

    public TracksPage(IUnitOfWork unitOfWork)
    {
        InitializeComponent();

        service = new TrackService(unitOfWork);

        BindingContext = this;

        _ = LoadTracks();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _ = LoadTracks();
    }

    private async Task LoadTracks()
    {
        var tracks = await service.GetAllAsync();

        if (tracks.Count == 0)
            tracks.Add(new Track
            {
                Guid = Guid.NewGuid().ToString(),
                Name = "Demo Track",
                Observation = "This is a demo track.",
                Created = DateTime.UtcNow.AddMinutes(-35).ToString("o"),
                Finalized = DateTime.UtcNow.ToString("o"),
                Length = 1234.5,
                Area = null,
                IsOpened = true.ToString(),
                TotalPoints = 3,
                LengthUnits = UnitEnum.Metros.ToString(),
            });

        Tracks.Clear();
        foreach (var track in tracks)
            Tracks.Add(track);

        Title = $"Tracks ({tracks.Count})";
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Track track)
        {
            await Navigation.PushAsync(new TrackDetailPage(track, TrackDetailPageMode.Edited));
        }
    }

    private async void OnViewOnMapClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Track track)
        {
            //navega a la página del mapa y muestra el track
            await Shell.Current.GoToAsync($"//MainPage?trackGuid={track.Guid}");
        }
    }
}