using WayPrecision.Domain.Data.UnitOfWork;
using WayPrecision.Domain.Models;
using WayPrecision.Domain.Services;

namespace WayPrecision;

public enum WaypointDetailPageMode
{
    Created,
    Edited
}

/// <summary>
/// Página de detalle para crear o editar un waypoint.
/// Permite visualizar, modificar, guardar y eliminar waypoints.
/// </summary>
public partial class WaypointDetailPage : ContentPage
{
    /// <summary>
    /// Servicio para gestionar operaciones sobre waypoints.
    /// </summary>
    private readonly WaypointService service;

    /// <summary>
    /// Indica el modo de la página: creación o edición.
    /// </summary>
    private readonly WaypointDetailPageMode PageMode;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="WaypointDetailPage"/>.
    /// </summary>
    /// <param name="waypoint">El waypoint a mostrar o editar.</param>
    /// <param name="pageMode">Modo de la página: creación o edición.</param>
    public WaypointDetailPage(Waypoint waypoint, WaypointDetailPageMode pageMode)
    {
        InitializeComponent();

        IUnitOfWork _unitOfWork = ((App)Application.Current).Services.GetRequiredService<IUnitOfWork>();
        service = new WaypointService(_unitOfWork);

        BindingContext = waypoint;
        PageMode = pageMode;

        if (PageMode == WaypointDetailPageMode.Created)
        {
            DeleteButton.IsVisible = false;
            ViewOnMapButton.IsVisible = false;
        }
    }

    /// <summary>
    /// Maneja el evento de clic para eliminar el waypoint actual.
    /// Muestra confirmación antes de eliminar y navega hacia atrás tras la eliminación.
    /// </summary>
    private async void OnDeleteWaypointClicked(object sender, EventArgs e)
    {
        // Aquí puedes agregar la lógica de eliminación, por ejemplo mostrar confirmación
        bool confirm = await DisplayAlert("Confirmar", "¿Seguro que deseas eliminar este waypoint?", "Sí", "No");
        if (confirm)
        {
            // Lógica para eliminar el waypoint usando _unitOfWork, por ejemplo:
            await service.DeleteAsync((Waypoint)BindingContext);

            await DisplayAlert("Eliminado", "El waypoint ha sido eliminado.", "OK");
            await Navigation.PopAsync();
        }
    }

    /// <summary>
    /// Maneja el evento de clic para guardar el waypoint actual.
    /// Valida los datos y guarda según el modo de la página.
    /// </summary>
    private async void OnSaveWaypointClicked(object sender, EventArgs e)
    {
        var waypoint = (Waypoint)BindingContext;

        if (string.IsNullOrWhiteSpace(waypoint.Name))
        {
            await DisplayAlert("Error", "El nombre es obligatorio.", "OK");
            return;
        }

        if (PageMode == WaypointDetailPageMode.Created)
        {
            await service.AddAsync(waypoint);
            await DisplayAlert("Guardado", "El waypoint ha sido creado.", "OK");
        }
        else if (PageMode == WaypointDetailPageMode.Edited)
        {
            await service.UpdateAsync(waypoint);
            await DisplayAlert("Guardado", "El waypoint ha sido actualizado.", "OK");
        }

        await Navigation.PopAsync();
    }
}