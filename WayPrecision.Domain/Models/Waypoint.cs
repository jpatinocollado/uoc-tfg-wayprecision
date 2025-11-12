using System.ComponentModel;
using SQLite;

namespace WayPrecision.Domain.Models
{
    public class Waypoint : INotifyPropertyChanged
    {
        public Waypoint()
        {
            _isVisible = true;
        }

        [PrimaryKey]
        public string Guid { get; set; } = String.Empty;

        public string Name { get; set; } = String.Empty;
        public string Observation { get; set; } = String.Empty;
        public string Created { get; set; } = String.Empty;

        private bool _isVisible;

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    OnPropertyChanged(nameof(IsVisible));
                    OnPropertyChanged(nameof(GetEyeImage));
                }
            }
        }

        public string GetEyeImage => IsVisible ? "eyeopen32x32.png" : "eyeclose32x32.png";

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        [Indexed]
        public string PositionGuid { get; set; } = String.Empty;

        [Ignore] // SQLite-net no soporta navegación automática, pero puedes cargarla manualmente
        public virtual Position Position { get; set; } = new Position();

        [Ignore]
        public DateTime? CreatedLocal
        {
            get
            {
                if (DateTime.TryParse(Created, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var utcDate))
                {
                    return utcDate.ToLocalTime();
                }
                return null;
            }
        }
    }
}