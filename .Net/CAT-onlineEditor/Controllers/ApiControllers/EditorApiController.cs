using CAT.Data;
using CAT.Helpers;
using CAT.Models;
using CAT.Services;
using CAT.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Controller;
using Newtonsoft.Json.Linq;
using NuGet.Configuration;
using System.Collections.Specialized;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web;
using AutoMapper;
using System.Configuration;

namespace CAT.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("onlineeditor/api/[controller]")]
    public class EditorApiController : ControllerBase
    {
        private readonly CatConnector _catClientService;
        private readonly ILogger _logger;
        private readonly JobService _jobService;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IUserService _userService;

        public EditorApiController(CatConnector catClientService, JobService jobService, IConfiguration configuration, 
            ILogger<EditorApiController> logger, IHttpClientFactory httpClientFactory, IUserService userService)
        {
            _catClientService = catClientService;
            _jobService = jobService;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _userService = userService;
            _configuration = configuration;
        }

        private void SaveJobDataToSession(JobData jobData)
        {
            //store the jobData in the user session
            var OEJobs = HttpContext.Session.Get<List<JobData>>("OEJobs");
            if (OEJobs == null)
            {
                OEJobs = new List<JobData> { jobData };
                HttpContext.Session.Set<List<JobData>>("jobData", OEJobs);
            }
            else
            {
                int idx = OEJobs.FindIndex(o => o.jobId == jobData.jobId);
                if (idx >= 0)
                    OEJobs[idx] = jobData; //the job is reloaded
                else
                    OEJobs.Add(jobData);
            }

            _logger.LogInformation("session saved: {jobId}", jobData.jobId);
        }

        private JobData GetJobDataFromSession(int jobId)
        {
            var OEJobs = HttpContext.Session.Get<List<JobData>>("jobData");
            int idx = OEJobs.FindIndex(o => o.jobId == jobId);
            return OEJobs[idx];
        }

        [HttpGet("GetEditorData")]
        public async Task<IActionResult> GetEditorData(string urlParams)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync();

                var decryptedDarams = EncryptionHelper.DecryptString(urlParams);
                var queryParams = HttpUtility.ParseQueryString(decryptedDarams);
                //load the job
                var jobId = int.Parse(queryParams["jobId"]!);
                var jobData = await _jobService.GetJobData(jobId, currentUser);
                //save the job data into the session
                SaveJobDataToSession(jobData);

                var editorData = new
                {
                    jobData.jobId,
                    jobData.task,
                    jobData.user,
                    jobData.pmUser,
                    translationUnits = jobData!.translationUnits!.Select(tu => new {
                        source = CatUtils.CodedTextToGoogleTags(tu.source!),
                        tu.target
                    }).ToList<object>()
                };

                return Ok(editorData);
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                Console.WriteLine(ex.Message);
                return Problem("An error occurred while processing your request.\n\n" + ex.Message, null, 500);
            }
        }

        [HttpPost("SaveSegment")]
        public IActionResult SaveSegment([FromBody] dynamic model)
        {
            try
            {
                string urlParams = model.GetProperty("urlParams").GetString();
                int tuid = model.GetProperty("tuid").GetInt32();
                if (tuid < 0)
                    return Problem("Invalid tuid.");

                var target = model.GetProperty("target").GetString();
                var confirmed = model.GetProperty("confirmed").GetBoolean();
                var propagate = model.GetProperty("propagate").GetInt32();

                //the job data
                var jobId = QueryHelper.GetQuerystringIntParameter(urlParams, "jobId");
                var jobData = GetJobDataFromSession(jobId);

                _jobService.SaveSegment(jobData, tuid, target, confirmed, propagate);
                return Ok("Saved");
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                Console.WriteLine(ex.Message);
                return Problem("An error occurred while processing your request.", null, 500);
            }
        }


        [HttpPost("GetTMMatches")]
        public IActionResult GetTMMatches([FromBody] dynamic model)
        {
            try
            {
                string urlParams = model.GetProperty("urlParams").GetString();
                int tuid = model.GetProperty("tuid").GetInt32();
                if (tuid <= 0)
                    return Problem("Invalid tuid.");

                var ix = tuid - 1;
                //the job data
                var jobId = QueryHelper.GetQuerystringIntParameter(urlParams, "jobId");
                var jobData = GetJobDataFromSession(jobId);

                //Convert google tags to xliff tags
                var tu = jobData.translationUnits![ix];
                String sourceXml = CatUtils.CodedTextToTmx(tu.source!);
                String? precedingXml = null;
                if (ix > 0)
                {
                    tu = jobData.translationUnits[ix - 1];
                    precedingXml = CatUtils.CodedTextToTmx(tu.source!);
                }
                String? followingXml = null;
                if (ix < jobData.translationUnits.Count - 1)
                {
                    tu = jobData.translationUnits[ix + 1];
                    followingXml = CatUtils.CodedTextToTmx(tu.source!);
                }

                var tmMatches = _catClientService.GetTMMatches(jobData.tmAssignments!.ToArray(), sourceXml, precedingXml!, followingXml!);

                return Ok(tmMatches);
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                Console.WriteLine(ex.Message);
                return Problem("An error occurred while processing your request.", null, 500);
            }
        }

        [HttpPost("GetConcordance")]
        public async Task<IActionResult> GetConcordance([FromBody] dynamic model)
        {
            try
            {
                var ret = await Task.Run(() =>
                {
                    //the job data
                    string urlParams = model.GetProperty("urlParams").GetString();
                    var jobId = QueryHelper.GetQuerystringIntParameter(urlParams, "jobId");
                    var jobData = GetJobDataFromSession(jobId);

                    var searchText = model.GetProperty("searchText").GetString();
                    bool caseSensitive = model.GetProperty("caseSensitive").GetBoolean();
                    bool searchInTarget = model.GetProperty("searchInTarget").GetBoolean();

                    //get corpus entries
                    var tmMatches = _catClientService.GetConcordance(jobData.tmAssignments!.ToArray(), searchText,
                        caseSensitive, searchInTarget);

                    return Ok(tmMatches);
                });

                return ret;
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                Console.WriteLine(ex.Message);
                return Problem("An error occurred while processing your request.", null, 500);
            }
        }

        [HttpGet("DownloadJob")]
        public async Task<IActionResult> DownloadJob(string urlParams)
        {
            var decryptedDarams = EncryptionHelper.DecryptString(urlParams);
            var queryParams = HttpUtility.ParseQueryString(decryptedDarams);
            //load the job
            var jobId = int.Parse(queryParams["jobId"]!);

            var httpClient = _httpClientFactory.CreateClient();
            var catMainBaseUrl = _configuration["CATMainBaseUrl"];
            var response = await httpClient.GetAsync($"{catMainBaseUrl}/api/EditorApi/DownloadDocument/{jobId}");

            if (response.IsSuccessStatusCode)
            {
                var fileByteArray = await response.Content.ReadAsByteArrayAsync();
                var contentDispositionHeader = response.Content.Headers.ContentDisposition;
                var fileName = contentDispositionHeader?.FileName!.Trim('"') ?? "defaultFileName.ext";

                return File(fileByteArray, "application/octet-stream", fileName);
            }

            return BadRequest("Could not download the file.");            
        }

        [HttpGet("SubmitJob")]
        public async Task<IActionResult> SubmitJob(string urlParams)
        {
            var decryptedDarams = EncryptionHelper.DecryptString(urlParams);
            var queryParams = HttpUtility.ParseQueryString(decryptedDarams);
            //load the job
            var jobId = int.Parse(queryParams["jobId"]!);

            //get the user
            var currentUser = await _userService.GetCurrentUserAsync();

            var httpClient = _httpClientFactory.CreateClient();
            var catMainBaseUrl = _configuration["CATMainBaseUrl"];
            var response = await httpClient.GetAsync($"{catMainBaseUrl}/api/EditorApi/SubmitJob?jobId={jobId}&userId={currentUser.Id}");

            if (response.IsSuccessStatusCode)
                return Ok();

            return BadRequest("Unable to submit job.");
        }
    }
}
