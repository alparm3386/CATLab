using CATWeb.Data;
using CATWeb.Helpers;
using CATWeb.Services.CAT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Web;

namespace CATWeb.Controllers.ApiControllers
{
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    public class EditorApiController : ControllerBase
    {
        private readonly CATWebContext _context;
        private readonly IConfiguration _configuration;
        private readonly CATClientService _catClientService;
        private readonly IMemoryCache _cache;

        public EditorApiController(CATWebContext context, IConfiguration configuration, CATClientService catClientService, IMemoryCache cache)
        {
            _context = context;
            _configuration = configuration;
            _catClientService = catClientService;
            _cache = cache;
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
                string filterPath = null;
                if (!String.IsNullOrEmpty(job.FilterName))
                    filterPath = Path.Combine(fileFiltersFolder, job.FilterName);

                dynamic jobData = new
                {
                    translationUnits = translationUnits.Select(tu => new { source = CATUtils.XmlTags2GoogleTags(tu.sourceText, CATUtils.TagType.Tmx), 
                        target = tu.targetText })
                };

                //store the jobData in the user session
                HttpContext.Session.Set<dynamic>("jobData", (object)jobData);

                var editorData = new
                {
                    translationUnits = jobData.translationUnits
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

        [HttpPost("FetchTMMatches")]
        public IActionResult FetchTMMatches([FromBody] dynamic model)
        {
            try
            {
                string urlParams = model.GetProperty("urlParams").GetString();
                int tuid = model.GetProperty("tuid").GetInt32();

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
