using AutoMapper;
using CAT.Data;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;

namespace CAT.Services.Common
{
    public class MonitoringService : IMonitoringService
    {
        private readonly DbContextContainer _dbContextContainer;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public MonitoringService(DbContextContainer dbContextContainer, IConfiguration configuration,
            IMapper mapper, ILogger<JobService> logger)
        {
            _dbContextContainer = dbContextContainer;
            _configuration = configuration;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<dynamic> GetMonitoringData()
        {
            //the return object
            dynamic monitoringData = new ExpandoObject();
            monitoringData.orders = new List<dynamic>();

            //get the orders
            var orders = _dbContextContainer.MainContext.Orders
                .Include(o => o.Jobs)
                    .ThenInclude(j => j.Quote)
                .Include(o => o.Jobs)
                    .ThenInclude(j => j.WorkflowSteps);

            //process orders
            foreach (var dsOrder in orders)
            {
                //the order object
                dynamic order = new ExpandoObject();
                order.id = dsOrder.Id;
                order.client = dsOrder.ClientId;
                order.dateCreated = dsOrder.DateCreated;
                order.jobs = new List<dynamic>();

                foreach (var dsJob in dsOrder.Jobs)
                {
                    //the job object
                    dynamic job = new ExpandoObject();
                }

                monitoringData.orders.Add(order);
            }


            return monitoringData;
        }
    }
}
