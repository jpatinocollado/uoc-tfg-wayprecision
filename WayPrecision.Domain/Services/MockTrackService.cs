using WayPrecision.Domain.Models;

namespace WayPrecision.Domain.Services
{
    public class MockTrackService : IService<Track>
    {
        private List<Track> Tracks = new List<Track>();

        public MockTrackService()
        {
            // Initialize with some mock data
            string trackId = Guid.NewGuid().ToString();
            string position1 = Guid.NewGuid().ToString();
            string position2 = Guid.NewGuid().ToString();
            string position3 = Guid.NewGuid().ToString();

            Tracks.Add(new Track
            {
                Guid = trackId,
                Name = "Demo Track",
                Observation = "This is a demo track.",
                Created = DateTime.UtcNow.AddMinutes(-35).ToString("o"),
                Finalized = DateTime.UtcNow.ToString("o"),
                Length = 1234.5,
                Area = null,
                IsOpened = false,
                TotalPoints = 3,
                LengthUnits = UnitEnum.Metros.ToString(),
                TypeGeometry = TypeGeometry.Polygon,
                TrackPoints = new List<TrackPoint>() {
                    new TrackPoint(){
                         Guid = Guid.NewGuid().ToString(),
                         TrackGuid = trackId,
                         PositionGuid = position1,
                         Position = new Position(){
                            Guid = position1,
                            Latitude = 41.661003770342276,
                            Longitude = 0.5547822911068457,
                            Timestamp = DateTime.UtcNow.ToString("o"),
                            Accuracy = 20
                         }
                    },
                    new TrackPoint(){
                         Guid = Guid.NewGuid().ToString(),
                         TrackGuid = trackId,
                         PositionGuid = position2,
                         Position = new Position(){
                            Guid = position1,
                            Latitude = 41.6605906511649,
                            Longitude = 0.5552997755430279,
                            Timestamp = DateTime.UtcNow.AddSeconds(5).ToString("o"),
                            Accuracy = 20
                         }
                    },
                    new TrackPoint(){
                         Guid = Guid.NewGuid().ToString(),
                         TrackGuid = trackId,
                         PositionGuid = position3,
                         Position = new Position(){
                            Guid = position1,
                            Latitude = 41.661350708186475,
                            Longitude = 0.5555705834603986,
                            Timestamp = DateTime.UtcNow.AddSeconds(10).ToString("o"),
                            Accuracy = 20
                         }
                    }
                }
            });

            string t2 = Guid.NewGuid().ToString();
            string t2p1 = Guid.NewGuid().ToString();
            string t2p2 = Guid.NewGuid().ToString();
            string t2p3 = Guid.NewGuid().ToString();
            string t2p4 = Guid.NewGuid().ToString();

            Tracks.Add(new Track
            {
                Guid = t2,
                Name = "Demo Track 2",
                Observation = "This is a 2 demo track.",
                Created = DateTime.UtcNow.AddMinutes(-35).ToString("o"),
                Finalized = DateTime.UtcNow.ToString("o"),
                Length = 1234.5,
                Area = null,
                IsOpened = false,
                TotalPoints = 3,
                LengthUnits = UnitEnum.Metros.ToString(),
                TypeGeometry = TypeGeometry.Polygon,
                TrackPoints = new List<TrackPoint>() {
                    new TrackPoint(){
                         Guid = Guid.NewGuid().ToString(),
                         TrackGuid = t2,
                         PositionGuid = t2p1,
                         Position = new Position(){
                            Guid = t2p1,
                            Latitude = 41.66215253129646,
                            Longitude = 0.5533226915521051,
                            Timestamp = DateTime.UtcNow.ToString("o"),
                            Accuracy = 20
                         }
                    },
                    new TrackPoint(){
                         Guid = Guid.NewGuid().ToString(),
                         TrackGuid = t2,
                         PositionGuid = t2p2,
                         Position = new Position(){
                            Guid = t2p2,
                            Latitude = 41.662429274617224,
                            Longitude = 0.5539554704481465,
                            Timestamp = DateTime.UtcNow.AddSeconds(5).ToString("o"),
                            Accuracy = 20
                         }
                    },

                    new TrackPoint(){
                         Guid = Guid.NewGuid().ToString(),
                         TrackGuid = t2,
                         PositionGuid = t2p4,
                         Position = new Position(){
                            Guid = t2p4,
                            Latitude = 41.66162310597749,
                            Longitude = 0.5542021469669579,
                            Timestamp = DateTime.UtcNow.AddSeconds(15).ToString("o"),
                            Accuracy = 20
                         }
                    },
                    new TrackPoint(){
                         Guid = Guid.NewGuid().ToString(),
                         TrackGuid = t2,
                         PositionGuid = t2p3,
                         Position = new Position(){
                            Guid = t2p3,
                            Latitude = 41.661358391685724,
                            Longitude = 0.5535800931369118,
                            Timestamp = DateTime.UtcNow.AddSeconds(10).ToString("o"),
                            Accuracy = 20
                         }
                    }
                }
            });
        }

        public Task<Track> CreateAsync(Track entity)
        {
            Tracks.Add(entity);

            return Task.FromResult(entity);
        }

        public Task<bool> DeleteAsync(string guid)
        {
            var track = Tracks.FirstOrDefault(t => t.Guid == guid);
            if (track != null)
            {
                Tracks.Remove(track);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<List<Track>> GetAllAsync()
        {
            return Task.FromResult(Tracks);
        }

        public Task<Track> GetByIdAsync(string guid)
        {
            var track = Tracks.FirstOrDefault(t => t.Guid == guid);
            return Task.FromResult(track);
        }

        public Task<Track> UpdateAsync(Track entity)
        {
            var existingTrack = Tracks.FirstOrDefault(t => t.Guid == entity.Guid);
            if (existingTrack != null)
            {
                Tracks.Remove(existingTrack);
                Tracks.Add(entity);
                return Task.FromResult(entity);
            }
            return Task.FromResult<Track>(null);
        }
    }
}