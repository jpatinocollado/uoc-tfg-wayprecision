using WayPrecision.Domain.Data.UnitOfWork;
using WayPrecision.Domain.Models;
using WayPrecision.Domain.Services;

namespace WayPrecision;

public enum TrackDetailPageMode
{
    Created,
    Edited
}

public partial class TrackDetailPage : ContentPage
{
    /// <summary>
    /// Servicio para gestionar operaciones sobre tracks.
    /// </summary>
    private readonly IService<Track> service;

    /// <summary>
    /// Indica el modo de la página: creación o edición.
    /// </summary>
    private readonly TrackDetailPageMode PageMode;

    public TrackDetailPage(Track track, TrackDetailPageMode pageMode)
    {
        InitializeComponent();

        service = ((App)Application.Current).Services.GetRequiredService<IService<Track>>();

        BindingContext = track;
        PageMode = pageMode;

        if (PageMode == TrackDetailPageMode.Created)
        {
            DeleteButton.IsVisible = false;
            ViewOnMapButton.IsVisible = false;
        }
    }

    /// <summary>
    /// Maneja el evento de clic para eliminar el track actual.
    /// Muestra confirmación antes de eliminar y navega hacia atrás tras la eliminación.
    /// </summary>
    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        // Aquí puedes agregar la lógica de eliminación, por ejemplo mostrar confirmación
        bool confirm = await DisplayAlert("Confirmar", "¿Seguro que deseas eliminar este track?", "Sí", "No");
        if (confirm)
        {
            // Lógica para eliminar el track usando _unitOfWork, por ejemplo:
            Track track = (Track)BindingContext;
            await service.DeleteAsync(track.Guid);

            await DisplayAlert("Eliminado", "El track ha sido eliminado.", "OK");

            await Shell.Current.GoToAsync($"//MainPage");
        }
    }

    /// <summary>
    /// Maneja el evento de clic para guardar el track actual.
    /// Valida los datos y guarda según el modo de la página.
    /// </summary>
    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var track = (Track)BindingContext;

        if (string.IsNullOrWhiteSpace(track.Name))
        {
            await DisplayAlert("Error", "El nombre es obligatorio.", "OK");
            return;
        }

        if (PageMode == TrackDetailPageMode.Created)
        {
            await service.CreateAsync(track);
            await DisplayAlert("Guardado", "El track ha sido creado.", "OK");
        }
        else if (PageMode == TrackDetailPageMode.Edited)
        {
            await service.UpdateAsync(track);
            await DisplayAlert("Guardado", "El track ha sido actualizado.", "OK");
        }

        await Shell.Current.GoToAsync($"//MainPage?trackGuid={track.Guid}");
    }

    private async void ViewOnMapClicked(object sender, EventArgs e)
    {
        Track track = (Track)BindingContext;
        await Shell.Current.GoToAsync($"//MainPage?trackGuid={track.Guid}");
    }
}