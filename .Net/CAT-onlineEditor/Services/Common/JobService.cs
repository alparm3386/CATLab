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
using CAT.Models.Entities.Main;
using CAT.Areas.Identity.Data;

namespace CAT.Services.Common
{
    public class JobService
    {
        private readonly DbContextContainer _dbContextContainer;
        private readonly CATConnector _catConnector;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public JobService(DbContextContainer dbContextContainer, CATConnector catConnector, 
            IMapper mapper, ILogger<JobService> logger)
        {
            _dbContextContainer = dbContextContainer;
            _catConnector = catConnector;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<JobData> GetJobData(int jobId, ApplicationUser user)
        {
            try
            {
                //get the job
                var job = await _dbContextContainer.MainContext.Jobs.AsNoTracking().Include(j => j.Quote).Where(j => j.Id == jobId).FirstOrDefaultAsync();

                //get the workflow
                var workflowSteps = await _dbContextContainer.MainContext.WorkflowSteps.Where(ws => ws.JobId == jobId).AsNoTracking().ToListAsync();

                //check if the job was processed
                var newJobStep = workflowSteps.Find(ws => ws.TaskId == (int)Enums.Task.NewJob);
                if (newJobStep?.Status < 2)
                    throw new Exception("The job has not been processed yet.");

                //get the current task
                var currentStep = workflowSteps.Find(ws => ws.Status == (int)WorkflowStatus.InProgress);

                //check the allocation
                if (user.UserType != UserType.Admin)
                {
                    var allocation = await _dbContextContainer.MainContext.Allocations
                        .FirstOrDefaultAsync(allocation => allocation.JobId == jobId && allocation.UserId == user.Id);
                    _ = allocation ?? throw new Exception("The job is not allocated.");
                }

                //document
                var document = _dbContextContainer.MainContext.Documents.Find(job!.SourceDocumentId);

                //load the translation units
                var translationUnits = await _dbContextContainer.TranslationUnitsContext.TranslationUnit
                                 .Where(tu => tu.documentId == document!.Id).OrderBy(tu => tu.tuid).ToListAsync();

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
                    jobId = jobId,
                    translationUnits = translationUnitDTOs.ToList(),
                    tmAssignments = new List<TMAssignment>(tmAssignments),
                    tbAssignments = null!,
                    task = currentStep!.TaskId,
                    user = new { user.FullName, user.UserType },
                    pmUser = new { FullName = "Christina von der Leyen", UserType.Admin }
                };

                return jobData;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static long GetMaskForTask(JobData jobData)
        {
            return jobData.jobId | 0xfL; // dummy return
        }

        public int[] SaveSegment(JobData jobData, int tuid, String sTarget, bool bConfirmed, int propagate)
        {
            Stopwatch sw = new();
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
            var aRet = new List<int>();
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
                        tmpTu.status = tu.status | mask;
                        _dbContextContainer.TranslationUnitsContext.TranslationUnit.Update(tmpTu);
                    }
                }

                _dbContextContainer.TranslationUnitsContext.SaveChanges();
            }

            //update the progress
            // UpdateJobProgress(jobData)

            //Add the translation to the TMs
            if (bConfirmed)
                AddTMItem(jobData, tuid, sTarget);

            sw.Stop();
            if (sw.ElapsedMilliseconds > 1000)
            {
                _logger.LogInformation("SaveSegment completed in {ElapsedMilliseconds} ms for jobId: {JobId}", sw.ElapsedMilliseconds, jobData.jobId);
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
                        precedingXml = CATUtils.CodedTextToTmx(tu.source!);
                    }
                    String followingXml = null!;
                    if (ix < jobData.translationUnits.Count - 1)
                    {
                        tu = jobData.translationUnits[ix + 1];
                        followingXml = CATUtils.CodedTextToTmx(tu.source!);
                    }

                    foreach (var tmAssignment in jobData.tmAssignments!)
                    {
                        if (!tmAssignment.isReadonly && !tmAssignment.isGlobal)
                        {
                            var user = "0_2104";
                            var idSpeciality = 1;
                            var metadata = new Dictionary<String, String>() { { "user", user },
                            { "jobId", jobData.jobId.ToString() }, { "speciality", idSpeciality.ToString() } };
                            _catConnector.AddTMEntry(tmAssignment, sourceXml, targetXml, precedingXml!, followingXml!, metadata);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("jobId: {jobId} {exception}", jobData.jobId, ex.ToString());
                }
            });
        }

    }
}
