using AutoMapper;
using CATService;
using CAT.Controllers.ApiControllers;
using CAT.Data;
using CAT.Enums;
using CAT.Helpers;
using CAT.Models;
using CAT.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using static ICSharpCode.SharpZipLib.Zip.ZipEntryFactory;

namespace CAT.Services.CAT
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
            CATClientService catClientService, IMemoryCache cache, IMapper mapper, ILogger<JobService> logger)
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
            foreach (var tu in translationUnitDTOs)
            {
                tu.isEditAllowed = true;
            }

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

        private static long GetMaskForTask(JobData jobDataData)
        {
            //var task = jobDataData.task;
            ////if the user is admin then we need the latest task done by the linguists or the client reviewer
            //if (editorData.userType == userType.administrator)
            //{
            //    if (editorData.task == Task.Delivery || editorData.task == Task.CreditTranslators
            //        || editorData.task == Task.Payment || editorData.task == Task.End || editorData.task == Task.SpecialReview
            //        || editorData.task == Task.SpecialReview2 || editorData.task == Task.CheckAfterReview)
            //    {
            //        if (editorData.withClientReview)
            //            task = Task.SpecialReview;
            //        else if (editorData.withRevision)
            //            task = Task.Proofread;
            //        else
            //            task = Task.Translate;
            //    }
            //    else if (editorData.task == Task.Proofread || editorData.task == Task.ReceiveCompletedJob
            //        || editorData.task == Task.Completed)
            //    {
            //        if (editorData.withRevision)
            //            task = Task.Proofread;
            //        else
            //            task = Task.Translate;
            //    }
            //    else if (editorData.task == Task.Translate || editorData.task == Task.ProofreaderJB)
            //        task = Task.Translate;
            //}
            //var mask = 0x0;

            //switch (task)
            //{
            //    case Task.Translate:
            //    case Task.Transcreation:
            //    case Task.MTRevise:
            //        mask = 1; break;
            //    case Task.Proofread:
            //    case Task.TranscreationRevision:
            //    case Task.RevisionOfCopyRevision:
            //        mask = 2; break;
            //    case Task.SpecialReview:
            //    case Task.SpecialReview2:
            //        mask = 4; break;
            //    case Task.Reworking:
            //        mask = 8; break;
            //}

            //return mask;
            return 0xfL;
        }

        public int[] SaveSegment(JobData jobData, int tuid, String sTarget, bool bConfirmed, int propagate)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            var ix = tuid - 1;
            //get the translation unit
            var tuDto = jobData.translationUnits![ix];
            if (!tuDto.isEditAllowed)
                throw new Exception("Not allowed to edit the segment.");

            //set the status
            var mask = GetMaskForTask(jobData);
            tuDto.status = bConfirmed ? tuDto.status | mask : tuDto.status & ~mask;

            tuDto.target = sTarget;
            List<int> aRet = new List<int>();
            //save the translated segment
            using (var dbContext = _context)
            {
                var tu = _mapper.Map<TranslationUnit>(tuDto);
                _context.TranslationUnit.Update(tu);

                //do the auto-propagation
                if (propagate > 0 && propagate < 3 && bConfirmed)
                {
                    int from = propagate == 1 ? ix : 0;
                    for (int i = from; i < jobData.translationUnits.Count; i++)
                    {
                        var tmpTuDto = jobData.translationUnits[i];
                        if (i == ix || tmpTuDto.source != tu.source)
                            continue;

                        var tmpTu = _mapper.Map<TranslationUnit>(tu);
                        //update the segment
                        tmpTu.status = bConfirmed ? tu.status | mask : tu.status & ~mask;
                        _context.TranslationUnit.Update(tmpTu);
                    }
                }

                _context.SaveChanges();
            }

            //update the progress
            // UpdateJobProgress(jobData);

            //Add the translation to the TMs
            if (bConfirmed)
                AddTMItem(jobData, tuid, sTarget);

            sw.Stop();
            if (sw.ElapsedMilliseconds > 1000)
            {
                _logger.LogInformation("SaveSegment completed in " + sw.ElapsedMilliseconds +
                    " idJob: " + jobData.idJob.ToString());
            }

            return aRet.ToArray();
        }

        public void AddTMItem(JobData jobData, int tuid, String sTarget)
        {
            //check the TMs
            if (jobData.tmAssignments?.Count == 0)
                return;
            //update the TMs in a separate thread
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    var ix = tuid - 1;
                    var tu = jobData.translationUnits![ix];

                    //Convert google tags to xliff tags
                    String sourceXml = CATUtils.CodedTextToTmx(tu.source!);
                    var tagsMap = CATUtils.GetTagsMap(tu.source!);
                    String targetXml = CATUtils.GoogleTagsToTmx(sTarget, tagsMap);
                    String? precedingXml = null;
                    if (ix > 0)
                    {
                        tu = jobData.translationUnits[ix - 1];
                        tagsMap = CATUtils.GetTagsMap(tu.source!);
                        precedingXml = CATUtils.CodedTextToTmx(tu.source!);
                    }
                    String followingXml = null;
                    if (ix < jobData.translationUnits.Count - 1)
                    {
                        tu = jobData.translationUnits[ix + 1];
                        tagsMap = CATUtils.GetTagsMap(tu.source!);
                        followingXml = CATUtils.CodedTextToTmx(tu.source!);
                    }

                    //var catConnector = CATConnectorFactory.CreateCATConnector(editorData.iServer);
                    foreach (var tmAssignment in jobData.tmAssignments!)
                    {
                        if (!tmAssignment.isReadonly && !tmAssignment.isGlobal)
                        {
                            var user = "0_2104";
                            var idSpeciality = 14;
                            var metadata = new Dictionary<String, String>() { { "user", user },
                            { "idTranslation", jobData.idJob.ToString() }, { "speciality", idSpeciality.ToString() } };
                            _catClientService.AddTMEntry(tmAssignment, sourceXml, targetXml, precedingXml, followingXml, metadata);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("idJob: " + jobData.idJob + " " + ex.ToString());
                }
            });
        }

    }
}
