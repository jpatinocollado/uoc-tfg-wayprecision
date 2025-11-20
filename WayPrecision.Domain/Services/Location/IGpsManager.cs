using WayPrecision.Domain.Models;

namespace WayPrecision.Domain.Services.Location
{
    public interface IGpsManager
    {
        Task StartListeningAsync(TimeSpan gpsInterval, int gpsMinimalAccuracy);

        Task StopListeningAsync();

        Task ChangeGpsInterval(TimeSpan gpsInterval);

        Task ChangeGpsMinimalAccuracy(int gpsMinimalAccuracy);

        event EventHandler<LocationEventArgs> PositionChanged;
    }
}