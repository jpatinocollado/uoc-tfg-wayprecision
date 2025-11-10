using WayPrecision.Domain.Data.UnitOfWork;
using WayPrecision.Domain.Models;
using WayPrecision.Domain.Pages;
using WayPrecision.Domain.Services;

namespace WayPrecision;

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
    private readonly DetailPageMode PageMode;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="WaypointDetailPage"/>.
    /// </summary>
    /// <param name="waypoint">El waypoint a mostrar o editar.</param>
    /// <param name="pageMode">Modo de la página: creación o edición.</param>
    public WaypointDetailPage(Waypoint waypoint, DetailPageMode pageMode)
    {
        InitializeComponent();

        IUnitOfWork _unitOfWork = ((App)Application.Current).Services.GetRequiredService<IUnitOfWork>();
        service = new WaypointService(_unitOfWork);

        BindingContext = waypoint;
        PageMode = pageMode;

        if (PageMode == DetailPageMode.Created)
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
            //await Navigation.PopAsync();

            //// Actualizar waypoints en MainPage si está en la pila de navegación
            //if (Application.Current.MainPage is NavigationPage navPage)
            //{
            //    var mainPage = navPage.Navigation.NavigationStack.OfType<MainPage>().FirstOrDefault();
            //    mainPage?.PaintWaypoints();
            //}

            await Shell.Current.GoToAsync($"//MainPage");
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

        if (PageMode == DetailPageMode.Created)
        {
            await service.AddAsync(waypoint);
            await DisplayAlert("Guardado", "El waypoint ha sido creado.", "OK");
        }
        else if (PageMode == DetailPageMode.Edited)
        {
            await service.UpdateAsync(waypoint);
            await DisplayAlert("Guardado", "El waypoint ha sido actualizado.", "OK");
        }

        //await Navigation.PopAsync();

        //// Actualizar waypoints en MainPage si está en la pila de navegación
        //if (Application.Current != null &&
        //        Application.Current.MainPage is Shell shell &&
        //                    shell.CurrentPage is MainPage mainPage)
        //    mainPage?.PaintWaypoints();

        await Shell.Current.GoToAsync($"//MainPage?waypointGuid={waypoint.Guid}");
    }

    private async void ViewOnMapClicked(object sender, EventArgs e)
    {
        Waypoint waypoint = (Waypoint)BindingContext;
        await Shell.Current.GoToAsync($"//MainPage?waypointGuid={waypoint.Guid}");
    }
}