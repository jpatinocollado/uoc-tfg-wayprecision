using System.ComponentModel;
using WayPrecision.Domain.Models;

namespace WayPrecision.Pages.Tracks
{
    public class TrackListItem : INotifyPropertyChanged
    {
        public TrackListItem(Track track)
        {
            Track = track;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
               PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public Track Track { get; set; }

        public bool IsVisible
        {
            get => Track.IsVisible;
            set
            {
                if (Track.IsVisible != value)
                {
                    Track.IsVisible = value;
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
    }
}