using System.Collections.ObjectModel;
using WayPrecision.Domain.Data.UnitOfWork;
using WayPrecision.Domain.Models;
using WayPrecision.Domain.Pages;
using WayPrecision.Domain.Services;

namespace WayPrecision;

public partial class TracksPage : ContentPage
{
    private readonly IService<Track> _service;
    public ObservableCollection<Track> Tracks { get; set; } = [];

    public TracksPage(IService<Track> service)
    {
        InitializeComponent();

        _service = service;

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
        var tracks = await _service.GetAllAsync();

        Tracks.Clear();
        foreach (var track in tracks)
            Tracks.Add(track);

        Title = $"Tracks ({tracks.Count})";
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Track track)
        {
            await Navigation.PushAsync(new TrackDetailPage(track, DetailPageMode.Edited));
        }
    }

    private async void OnViewOnMapClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Track track)
        {
            if (!track.IsVisible)
                OnEyeClicked(sender, new EventArgs());

            //Task.Delay(500).Wait();

            //navega a la página del mapa y muestra el track
            await Shell.Current.GoToAsync($"//MainPage?trackGuid={track.Guid}");
        }
    }

    private async void OnEyeClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Track track)
        {
            track.IsVisible = !track.IsVisible;

            await _service.UpdateAsync(track);
        }
    }
}