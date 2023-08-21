using AutoMapper;
using Microsoft.Extensions.Caching.Memory;

namespace CAT.Services.CAT
{
    public class WorkflowService
    {
        public WorkflowService(IdentityDbContext identityDBContext, MainDbContext mainDbContext, TranslationUnitsDbContext translationUnitsDbContext,
            IConfiguration configuration, CATConnector catConnector, IMemoryCache cache, IMapper mapper, ILogger<JobService> logger)
        { 
        }
    }
}
