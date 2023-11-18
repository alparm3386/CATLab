using AutoMapper;
using CAT.Controllers.Api;
using CAT.Data;
using CAT.Enums;
using CAT.Helpers;
using CAT.Models;
using CAT.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using static ICSharpCode.SharpZipLib.Zip.ZipEntryFactory;
using CAT.Models.Entities.TranslationUnits;
using CAT.Models.Common;
using System.Net.Http;

namespace CAT.Services.Common
{
    public class JobService
    {
        private readonly DbContextContainer _dbContextContainer;
        private readonly IConfiguration _configuration;
        private readonly CATConnector _catConnector;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public JobService(DbContextContainer dbContextContainer, IConfiguration configuration, CATConnector catConnector, IHttpClientFactory httpClientFactory,
            IMapper mapper, ILogger<JobService> logger)
        {
            _dbContextContainer = dbContextContainer;
            _configuration = configuration;
            _catConnector = catConnector;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<JobData> GetJobData(int jobId)
        {
            var job = await _dbContextContainer.MainContext.Jobs.Include(j => j.Quote).Where(j => j.Id == jobId).FirstOrDefaultAsync();

            //job process
            var jobProcess = await _dbContextContainer.MainContext.JobProcesses.Where(jp => jp.JobId == jobId).FirstOrDefaultAsync();

            //check if the job was processed
            if (jobProcess?.ProcessEnded == null)
            {
                //process the job in the main application
                var httpClient = _httpClientFactory.CreateClient();
                var catMainBaseUrl = _configuration["CATMainBaseUrl"];
                var response = await httpClient.GetAsync($"{catMainBaseUrl}/api/EditorApi/ProcessJob/{jobId}");

                if (!response.IsSuccessStatusCode)
                    throw new Exception("Sorry, we encountered an unexpected internal error.");
            }

            //load the translation units
            var translationUnits = await _dbContextContainer.TranslationUnitsContext.TranslationUnit
                             .Where(tu => tu.idJob == jobId).OrderBy(tu => tu.tuid).ToListAsync();

            var translationUnitDTOs = _mapper.Map<TranslationUnitDTO[]>(translationUnits);
            foreach (var tu in translationUnitDTOs)
            {
                tu.isEditAllowed = true;
            }

            //get the TMs
            var order = await _dbContextContainer.MainContext.Orders.Include(o => o.Client).AsNoTracking().Where(o => o.Id == job.OrderId).FirstAsync();
            var tmAssignments = _catConnector.GetTMAssignments(order.Client.CompanyId, job.Quote!.SourceLanguage, job.Quote!.TargetLanguage,
                job.Quote!.Speciality, true);

            var jobData = new JobData
            {
                idJob = jobId,
                translationUnits = translationUnitDTOs.ToList(),
                tmAssignments = new List<TMAssignment>(tmAssignments),
                tbAssignments = null!
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
            using (var dbContext = _dbContextContainer.TranslationUnitsContext)
            {
                var tu = _mapper.Map<TranslationUnit>(tuDto);
                _dbContextContainer.TranslationUnitsContext.TranslationUnit.Update(tu);

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
                        _dbContextContainer.TranslationUnitsContext.TranslationUnit.Update(tmpTu);
                    }
                }

                _dbContextContainer.TranslationUnitsContext.SaveChanges();
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
                    String followingXml = null!;
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
                            var idSpeciality = 1;
                            var metadata = new Dictionary<String, String>() { { "user", user },
                            { "jobId", jobData.idJob.ToString() }, { "speciality", idSpeciality.ToString() } };
                            _catConnector.AddTMEntry(tmAssignment, sourceXml, targetXml, precedingXml!, followingXml!, metadata);
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
