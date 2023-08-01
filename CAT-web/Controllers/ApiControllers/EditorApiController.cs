using CATWeb.Data;
using CATWeb.Helpers;
using CATWeb.Models;
using CATWeb.Services;
using CATWeb.Services.CAT;
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

namespace CATWeb.Controllers.ApiControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EditorApiController : ControllerBase
    {
        private readonly CATClientService _catClientService;
        private readonly IMemoryCache _cache;
        private readonly ILogger _logger;
        private readonly JobService _jobService;

        public EditorApiController(CATClientService catClientService, IMemoryCache cache, JobService jobService, 
            ILogger<EditorApiController> logger)
        {
            _catClientService = catClientService;
            _cache = cache;
            _logger = logger;
            _jobService = jobService;
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
                NameValueCollection queryParams = HttpUtility.ParseQueryString(decryptedDarams);
                //load the job
                var idJob = int.Parse(queryParams["idJob"]);
                var jobData = await _jobService.GetJobData(idJob);
                //save the job data into the session
                SaveJobDataToSession(jobData);

                var editorData = new
                {
                    translationUnits = jobData!.translationUnits!.Select(tu => new {
                        source = CATUtils.XmlTags2GoogleTags(tu.sourceText, CATUtils.TagType.Tmx),
                        target = tu.targetText
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

        [HttpPost("GetTMMatches")]
        public IActionResult GetTMMatches([FromBody] dynamic model)
        {
            try
            {
                string urlParams = model.GetProperty("urlParams").GetString();
                int tuid = model.GetProperty("tuid").GetInt32();
                
                //the job data
                var idJob = QueryHelper.GetQuerystringIntParameter(urlParams, "idJob");
                var jobData = GetJobDataFromSession(idJob);

                //Convert google tags to xliff tags
                //TranslationUnit tu = jobData.translationUnits[tuid];
                //String sSource = tu.sourceText;
                //String sourceXml = CATUtils.GoogleTags2XmlTags(tu.source, tu.tags);
                //String precedingXml = null;
                //if (idSegment > 0)
                //{
                //    tu = editorData.translationUnits[idSegment - 1];
                //    precedingXml = CATUtils.GoogleTags2XmlTags(tu.source, tu.tags);
                //}
                //String followingXml = null;
                //if (idSegment < editorData.translationUnits.Length - 1)
                //{
                //    tu = editorData.translationUnits[idSegment + 1];
                //    followingXml = CATUtils.GoogleTags2XmlTags(tu.source, tu.tags);
                //}

                //_catClientService.GetTMMatches(jobData.tmAssignments, );

                var tmMatches = new[]
                {
                    new { source = "Celestial Print Velour Sleepsuit and Hat Set", target = "Lot de combinaison et chapeau en velours à imprimé céleste", quality = 101, origin = "TM" },
                    new { source = "This velour set may be the star of their cosy collection!", target = "Cet ensemble en velours est peut-être la star de leur collection cosy !", quality = 85, origin = "TM" },
                    new { source = "Harry Potter™ Gryffindor Phone Case", target = "Coque de téléphone Harry Potter™ Gryffondor", quality = 75, origin = "TM" },
                    new { source = "Put your house pride on full display with this case", target = "Mettez la fierté de votre maison à l'honneur avec cet étui", quality = 50, origin = "TM" },
                };

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
        public IActionResult GetConcordance([FromBody] dynamic model)
        {
            try
            {
                string urlParams = model.GetProperty("urlParams").GetString();
                int tuid = model.GetProperty("tuid").GetInt32();

                var idJob = QueryHelper.GetQuerystringIntParameter(urlParams, "idJob");
                var jobData = GetJobDataFromSession(idJob);
                //_catClientService.GetTMMatches();

                var tmMatches = new[]
                {
                    new { source = "Celestial Print Velour Sleepsuit and Hat Set", target = "Lot de combinaison et chapeau en velours à imprimé céleste", quality = 101, origin = "TM" },
                    new { source = "This velour set may be the star of their cosy collection!", target = "Cet ensemble en velours est peut-être la star de leur collection cosy !", quality = 85, origin = "TM" },
                    new { source = "Harry Potter™ Gryffindor Phone Case", target = "Coque de téléphone Harry Potter™ Gryffondor", quality = 75, origin = "TM" },
                    new { source = "Put your house pride on full display with this case", target = "Mettez la fierté de votre maison à l'honneur avec cet étui", quality = 50, origin = "TM" },
                };

                return Ok(tmMatches);
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                Console.WriteLine(ex.Message);
                return Problem("An error occurred while processing your request.", null, 500);
            }
        }
    }
}
