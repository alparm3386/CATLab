using AutoMapper;
using CAT.Data;
using Microsoft.Extensions.Caching.Memory;

namespace CAT.Services.Common
{
    public class WorkflowService : IWorkflowService
    {
        public WorkflowService(DbContextContainer dbContextContainer, IConfiguration configuration, CATConnector catConnector,
            IMapper mapper, ILogger<JobService> logger)
        {
        }

        public async Task CreateWorkflowAsync(int ordeId)
        {
            throw new NotImplementedException();
        }
    }
}
