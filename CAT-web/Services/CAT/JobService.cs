using AutoMapper;
using CATWeb.Controllers.ApiControllers;
using CATWeb.Data;
using CATWeb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CATWeb.Services.CAT
{
    public class JobService
    {
        private readonly CATWebContext _context;
        private readonly IConfiguration _configuration;
        private readonly CATClientService _catClientService;
        private readonly IMemoryCache _cache;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;


        public JobService(CATWebContext context, IConfiguration configuration,
            CATClientService catClientService, IMemoryCache cache, IMapper mapper, ILogger<EditorApiController> logger)
        {
            _context = context;
            _configuration = configuration;
            _catClientService = catClientService;
            _cache = cache;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<JobData> GetJobData(int idJob)
        {
            var job = await _context.Job.FindAsync(idJob);

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    //check if the job was processed
                    if (job?.DateProcessed == null)
                    {
                        //parse the document
                        _catClientService.ParseDoc(idJob);
                        job!.DateProcessed = DateTime.Now;

                        // Save changes in the database
                        await _context.SaveChangesAsync();

                        transaction.Commit();
                    }
                }
                catch (Exception)
                {
                    // An error occurred, roll back the transaction
                    transaction.Rollback();
                    throw;
                }
            }

            //load the translation units
            var translationUnits = await _context.TranslationUnit
                             .Where(tu => tu.idJob == idJob)
                             .ToListAsync();

            var sourceFilesFolder = Path.Combine(_configuration["SourceFilesFolder"]);
            var fileFiltersFolder = Path.Combine(_configuration["FileFiltersFolder"]);

            var filePath = Path.Combine(sourceFilesFolder, job!.FileName!);
            string? filterPath = null;
            if (!String.IsNullOrEmpty(job.FilterName))
                filterPath = Path.Combine(fileFiltersFolder, job.FilterName);

            var jobData = new JobData
            {
                idJob = idJob,
                translationUnits = translationUnits,
                tmAssignments =
                    new List<Models.CAT.TMAssignment>() { new Models.CAT.TMAssignment() { tmPath = "29610/__35462_en_fr" } },
                tbAssignments = null
            };

            return jobData;
        }
    }
}
