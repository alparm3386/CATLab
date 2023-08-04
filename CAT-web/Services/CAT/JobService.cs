using AutoMapper;
using CATWeb.Controllers.ApiControllers;
using CATWeb.Data;
using CATWeb.Enums;
using CATWeb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;

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

            var translationUnitDTOs = _mapper.Map<TranslationUnitDTO[]>(translationUnits);

            var sourceFilesFolder = Path.Combine(_configuration["SourceFilesFolder"]);
            var fileFiltersFolder = Path.Combine(_configuration["FileFiltersFolder"]);

            var filePath = Path.Combine(sourceFilesFolder, job!.FileName!);
            string? filterPath = null;
            if (!String.IsNullOrEmpty(job.FilterName))
                filterPath = Path.Combine(fileFiltersFolder, job.FilterName);


            var jobData = new JobData
            {
                idJob = idJob,
                translationUnits = translationUnitDTOs.ToList(),
                tmAssignments =
                    new List<Models.CAT.TMAssignment>() { new Models.CAT.TMAssignment() { tmPath = "29610/__35462_en_fr" } },
                tbAssignments = null
            };

            return jobData;
        }

        public async Task<int[]> SaveSegment(JobData jobData, int ix, String sTarget, bool bConfirmed, int propagate)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            //get the translation unit
            var tu = jobData.translationUnits![ix];
            /*            if (jobData.OEMode != OEMode.Contest && !tu.isEditAllowed)
                            throw new Exception("Not allowed to edit the segment.");

                        //set the status
                        int mask = GetMaskForTask(editorData);
                        tu.status = bConfirmed ? tu.status | mask : tu.status & ~mask;

                        tu.target = sTarget;
                        List<int> aRet = new List<int>();
                        //save the translated segment
                        DBFactory.InsertTranslatedSection(tu.idSource, sTarget, bConfirmed, tu.status, editorData.version);
                        aRet.Add(ix);

                        //temporary measurement of the OE version
                        DBFactory.InsertOEVersion(tu.idSource, (int)editorData.task, editorData.version, "");

                        //do the auto-propagation
                        if (propagate > 0 && propagate < 3 && bConfirmed)
                        {
                            int from = propagate == 1 ? ix : 0;
                            for (int i = from; i < editorData.translationUnits.Length; i++)
                            {
                                TranslationUnit tmpTu = editorData.translationUnits[i];
                                if (i == ix || tmpTu.source != tu.source)
                                    continue;

                                //update the segment
                                tmpTu.status = bConfirmed ? tu.status | mask : tu.status & ~mask;
                                DBFactory.InsertTranslatedSection(tmpTu.idSource, sTarget, bConfirmed, tmpTu.status, editorData.version);
                                //temporary measurement of the OE version
                                DBFactory.InsertOEVersion(tmpTu.idSource, (int)editorData.task, editorData.version, "");
                                aRet.Add(i);
                            }
                        }

                        //update the progress
                        UpdateJobProgress(editorData);

                        //Add the translation to the TMs
                        if (bConfirmed)
                            AddTMItem(editorData, ix, sTarget);

                        sw.Stop();
                        if (sw.ElapsedMilliseconds > 1000)
                            Logging.DEBUG_LOG("SavingTime.log", "SaveSegment completed in " + sw.ElapsedMilliseconds +
                                " idTranslation: " + editorData.idTranslation.ToString());

                        return aRet.ToArray();*/

            return null;
        }
    }
}
