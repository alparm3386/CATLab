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

namespace CAT.Controllers.Api
{
    //[Authorize]
    [ApiController]
    [Route("onlineeditor/api/[controller]")]
    public class EditorApiController : ControllerBase
    {
        private readonly CATConnector _catClientService;
        private readonly ILogger _logger;
        private readonly JobService _jobService;
        private readonly IMapper _mapper;
        private readonly IHttpClientFactory _httpClientFactory;

        public EditorApiController(CATConnector catClientService, JobService jobService, IMapper mapper, ILogger<EditorApiController> logger, 
            IHttpClientFactory httpClientFactory)
        {
            _catClientService = catClientService;
            _jobService = jobService;
            _mapper = mapper;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        private void SaveJobDataToSession(JobData jobData)
        {
            //store the jobData in the user session
            var OEJobs = HttpContext.Session.Get<List<JobData>>("OEJobs");
            if (OEJobs == null)
            {
                OEJobs = new List<JobData>();
                OEJobs.Add(jobData);
                HttpContext.Session.Set<List<JobData>>("jobData", OEJobs);
            }
            else
            {
                int idx = OEJobs.FindIndex(o => o.idJob == jobData.idJob);
                if (idx >= 0)
                    OEJobs[idx] = jobData; //the job is reloaded
                else
                    OEJobs.Add(jobData);
            }

            _logger.LogInformation("session saved: " + jobData.idJob);
        }

        private JobData GetJobDataFromSession(int idJob)
        {
            var OEJobs = HttpContext.Session.Get<List<JobData>>("jobData");
            int idx = OEJobs.FindIndex(o => o.idJob == idJob);
            return OEJobs[idx];
        }

        [HttpGet("GetEditorData")]
        public async Task<IActionResult> GetEditorData(string urlParams)
        {
            try
            {
                var decryptedDarams = EncryptionHelper.DecryptString(urlParams);
                var queryParams = HttpUtility.ParseQueryString(decryptedDarams);
                //load the job
                var idJob = int.Parse(queryParams["idJob"]!);
                var jobData = await _jobService.GetJobData(idJob);
                //save the job data into the session
                SaveJobDataToSession(jobData);

                var editorData = new
                {
                    translationUnits = jobData!.translationUnits!.Select(tu => new {
                        source = CATUtils.CodedTextToGoogleTags(tu.source!),
                        tu.target
                    }).ToList<object>()
                };

                return Ok(editorData);
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                Console.WriteLine(ex.Message);
                return Problem("An error occurred while processing your request.", null, 500);
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
                var idJob = QueryHelper.GetQuerystringIntParameter(urlParams, "idJob");
                var jobData = GetJobDataFromSession(idJob);

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
                var idJob = QueryHelper.GetQuerystringIntParameter(urlParams, "idJob");
                var jobData = GetJobDataFromSession(idJob);

                //Convert google tags to xliff tags
                var tu = jobData.translationUnits![ix];
                String sSource = tu.source!;
                String sourceXml = CATUtils.CodedTextToTmx(tu.source!);
                String? precedingXml = null;
                if (ix > 0)
                {
                    tu = jobData.translationUnits[ix - 1];
                    precedingXml = CATUtils.CodedTextToTmx(tu.source!);
                }
                String? followingXml = null;
                if (ix < jobData.translationUnits.Count - 1)
                {
                    tu = jobData.translationUnits[ix + 1];
                    followingXml = CATUtils.CodedTextToTmx(tu.source!);
                }

                var tmMatches = _catClientService.GetTMMatches(jobData.tmAssignments!.ToArray(), sourceXml, precedingXml!, followingXml!, null!);

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
                    var idJob = QueryHelper.GetQuerystringIntParameter(urlParams, "idJob");
                    var jobData = GetJobDataFromSession(idJob);

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
            var idJob = int.Parse(queryParams["idJob"]!);

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync($"https://localhost:7096/api/editorApi/downloadDocument/{idJob}");

            if (response.IsSuccessStatusCode)
            {
                var fileByteArray = await response.Content.ReadAsByteArrayAsync();
                var contentDispositionHeader = response.Content.Headers.ContentDisposition;
                var fileName = contentDispositionHeader?.FileName!.Trim('"') ?? "defaultFileName.ext";

                return File(fileByteArray, "application/octet-stream", fileName);
            }

            return BadRequest("Could not download the file.");
            
        }
    }
}
