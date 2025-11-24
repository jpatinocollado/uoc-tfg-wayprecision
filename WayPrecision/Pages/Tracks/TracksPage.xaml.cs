using System.Collections.ObjectModel;
using WayPrecision.Domain.Data.UnitOfWork;
using WayPrecision.Domain.Exceptions;
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
                Tracks.Add(track);

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
            if (sender is Button button && button.CommandParameter is Track track)
            {
                await Navigation.PushAsync(new TrackDetailPage(track, DetailPageMode.Edited));
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
            if (sender is Button btn && btn.CommandParameter is Track track)
            {
                if (!track.IsVisible)
                    OnEyeClicked(sender, new EventArgs());

                //navega a la página del mapa y muestra el track
                await Shell.Current.GoToAsync($"//MapPage?trackGuid={track.Guid}");
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
            if (sender is Button button && button.CommandParameter is Track track)
            {
                track.IsVisible = !track.IsVisible;

                await _service.UpdateAsync(track);
            }
        }
        catch (Exception ex)
        {
            GlobalExceptionManager.HandleException(ex, this);
        }
    }
}