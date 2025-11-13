using WayPrecision.Domain.Data;
using WayPrecision.Domain.Data.UnitOfWork;
using WayPrecision.Domain.Models;
using WayPrecision.Domain.Services;

namespace WayPrecision.Test
{
    public class TrackServiceIntegrationTest
    {
        private readonly IConfigurationService _configurationService;
        private readonly IService<Track> _service;
        private readonly IUnitOfWork _unitOfWork;

        public TrackServiceIntegrationTest()
        {
            var dbContext = new DatabaseContext("C:\\Users\\jpatinoc.INDRA\\AppData\\Local\\User Name\\com.companyname.wayprecision\\Data\\wayprecision.db3");
            _unitOfWork = new UnitOfWork(dbContext);
            _configurationService = new ConfigurationService(_unitOfWork);
            _service = new TrackService(_unitOfWork, _configurationService);
        }
    }
}