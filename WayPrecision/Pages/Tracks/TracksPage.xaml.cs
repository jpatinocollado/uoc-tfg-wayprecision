using System.Collections.ObjectModel;
using WayPrecision.Domain.Exceptions;
using WayPrecision.Domain.Models;
using WayPrecision.Domain.Pages;
using WayPrecision.Domain.Services;
using WayPrecision.Pages.Tracks;

namespace WayPrecision;

public partial class TracksPage : ContentPage
{
    private readonly IService<Track> _service;
    public ObservableCollection<TrackListItem> Tracks { get; set; } = [];

    public TracksPage(IService<Track> service)
    {
        InitializeComponent();

        _service = service;

        BindingContext = this;

        _ = LoadTracks();
    }

    protected override void OnAppearing()
    {
        try
        {
            base.OnAppearing();

            _ = LoadTracks();
        }
        catch (Exception ex)
        {
            GlobalExceptionManager.HandleException(ex, this);
        }
    }

    private async Task LoadTracks()
    {
        try
        {
            var tracks = await _service.GetAllAsync();

            Tracks.Clear();
            foreach (var track in tracks)
                Tracks.Add(new TrackListItem(track));

            Title = $"Tracks ({tracks.Count})";
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
            if (sender is Button button && button.CommandParameter is TrackListItem item)
            {
                await Navigation.PushAsync(new TrackDetailPage(item.Track, DetailPageMode.Edited));
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
            if (sender is Button btn && btn.CommandParameter is TrackListItem item)
            {
                if (!item.Track.IsVisible)
                    OnEyeClicked(sender, new EventArgs());

                //navega a la página del mapa y muestra el track
                await Shell.Current.GoToAsync($"//MapPage?trackGuid={item.Track.Guid}");
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
            if (sender is Button button && button.CommandParameter is TrackListItem item)
            {
                item.IsVisible = !item.IsVisible;

                await _service.UpdateAsync(item.Track);
            }
        }
        catch (Exception ex)
        {
            GlobalExceptionManager.HandleException(ex, this);
        }
    }
}