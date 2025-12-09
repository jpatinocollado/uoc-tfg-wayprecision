using System.ComponentModel;
using WayPrecision.Domain.Models;

namespace WayPrecision.Pages.Waypoints
{
    public class WaypointListItem : INotifyPropertyChanged
    {
        public Waypoint Waypoint { get; set; }

        public bool IsVisible
        {
            get => Waypoint.IsVisible;
            set
            {
                if (Waypoint.IsVisible != value)
                {
                    Waypoint.IsVisible = value;
                    OnPropertyChanged(nameof(IsVisible));
                    OnPropertyChanged(nameof(GetEyeImage));
                }
            }
        }

        public string GetEyeImage
        {
            get
            {
                string name = string.Empty;
                string tema = string.Empty;

                if (IsVisible)
                    name = "eyeopen32x32";
                else
                    name = "eyeclose32x32";

                if (Application.Current is not null &&
                    Application.Current.PlatformAppTheme == AppTheme.Dark)
                    tema = "dark";

                return $"{name}{tema}.png";
            }
        }

        public WaypointListItem(Waypoint waypoint)
        {
            Waypoint = waypoint;
            IsVisible = waypoint.IsVisible;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
               PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}