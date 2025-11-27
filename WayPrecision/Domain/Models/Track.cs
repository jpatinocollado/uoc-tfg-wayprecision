using System.ComponentModel;
using SQLite;
using WayPrecision.Domain.Helpers.Colors;

namespace WayPrecision.Domain.Models
{
    public class Track : INotifyPropertyChanged
    {
        private Configuration? _configuration;

        public Track()
        {
            _isVisible = true;
            ColorBorde = MapMarkerColorEnum.Red;
            ColorRelleno = MapMarkerColorEnum.Red;
        }

        public void SetConfiguration(Configuration configuration)
        {
            _configuration = configuration;
        }

        [PrimaryKey]
        public string Guid { get; set; } = String.Empty;

        public string Name { get; set; } = String.Empty;
        public string Observation { get; set; } = String.Empty;
        public string Created { get; set; } = String.Empty;
        public string Finalized { get; set; } = String.Empty;
        public bool IsOpened { get; set; }

        public bool IsManual { get; set; } = false;

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

        public string GetEyeImage
        {
            get
            {

                string name = string.Empty;
                string theme = string.Empty;

                if (IsVisible)
                    name = "eyeopen32x32";
                else
                    name = "eyeclose32x32";

                if (Application.Current is not null &&
                    Application.Current.RequestedTheme == AppTheme.Dark)
                    theme = "dark";

                return $"{name}{theme}.png";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        [Ignore]
        public int TotalPoints => TrackPoints.Count;

        public string AreaUnits { get; set; } = String.Empty;
        public string LengthUnits { get; set; } = String.Empty;

        public double? Area { get; set; }
        public double? Length { get; set; }

        public TypeGeometry TypeGeometry { get; set; }

        [Ignore]
        public string AreaLocal
        {
            get
            {
                if (!Area.HasValue)
                    return string.Empty;

                if (_configuration == null)
                    return $"{Math.Round(Area.Value, 2)} m²";

                if (!string.IsNullOrWhiteSpace(_configuration.AreaUnits) && _configuration.AreaUnits == UnitEnum.KilometrosCuadrados.ToString())
                {
                    double km2 = Area.Value / 1_000_000d;
                    return $"{Math.Round(km2, 4)} km²";
                }

                if (!string.IsNullOrWhiteSpace(_configuration.AreaUnits) && _configuration.AreaUnits == UnitEnum.Hectareas.ToString())
                {
                    double ha = Area.Value / 10_000d;
                    return $"{Math.Round(ha, 4)} ha";
                }

                //return fault metros cuadrados
                return $"{Math.Round(Area.Value, 2)} m²";
            }
        }

        [Ignore]
        public string LengthLocal
        {
            get
            {
                if (!Length.HasValue)
                    return string.Empty;

                if (_configuration == null)
                    return $"{Math.Round(Length.Value, 2)} m";

                if (!string.IsNullOrWhiteSpace(_configuration.LengthUnits) && _configuration.LengthUnits == UnitEnum.Kilometros.ToString())
                {
                    double km = Length.Value / 1_000d;
                    return $"{Math.Round(km, 4)} km";
                }

                return $"{Math.Round(Length.Value, 2)} m";
            }
        }

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

        [Ignore]
        public DateTime? FinalizedLocal
        {
            get
            {
                if (DateTime.TryParse(Finalized, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var utcDate))
                {
                    return utcDate.ToLocalTime();
                }
                return null;
            }
        }

        [Ignore]
        public TimeSpan Duration
        {
            get
            {
                if (CreatedLocal.HasValue && FinalizedLocal.HasValue)
                {
                    return FinalizedLocal.Value - CreatedLocal.Value;
                }
                return TimeSpan.Zero;
            }
        }

        [Ignore]
        public virtual List<TrackPoint> TrackPoints { get; set; } = new List<TrackPoint>();

        [Ignore]
        public MapMarkerColorEnum ColorBorde { get; set; }

        [Ignore]
        public MapMarkerColorEnum ColorRelleno { get; set; }

        public Track Clone()
        {
            return new Track
            {
                Guid = this.Guid,
                Name = this.Name,
                Observation = this.Observation,
                Created = this.Created,
                Finalized = this.Finalized,
                IsOpened = this.IsOpened,
                IsVisible = this.IsVisible,
                AreaUnits = this.AreaUnits,
                LengthUnits = this.LengthUnits,
                ColorBorde = this.ColorBorde,
                ColorRelleno = this.ColorRelleno,
                TypeGeometry = this.TypeGeometry,
                TrackPoints = new List<TrackPoint>()
            };
        }

        public void ReplacePositions(List<Position> positions)
        {
            TrackPoints.Clear();
            foreach (var pos in positions)
            {
                pos.Guid = System.Guid.NewGuid().ToString();
                TrackPoint smoothedTrackPoint = new()
                {
                    Guid = System.Guid.NewGuid().ToString(),
                    TrackGuid = this.Guid,
                    PositionGuid = pos.Guid,
                    Position = pos,
                };
                TrackPoints.Add(smoothedTrackPoint);
            }
        }
    }
}