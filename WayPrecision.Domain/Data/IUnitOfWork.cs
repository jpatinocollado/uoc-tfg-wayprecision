using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WayPrecision.Domain.Data.Repositories;

namespace WayPrecision.Domain.Data
{
    public interface IUnitOfWork
    {
        ConfigurationRepository Configurations { get; }
        UnitsRepository Units { get; }
        TracksRepository Tracks { get; }
        PositionsRepository Positions { get; }
        TrackPointRepository TrackPoints { get; }
        WaypointsRepository Waypoints { get; }

        Task BeginTransactionAsync();

        Task CommitTransactionAsync();

        Task RollbackTransactionAsync();

        Task<int> SaveChangesAsync();
    }
}