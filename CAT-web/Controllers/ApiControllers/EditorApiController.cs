using CAT_web.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Web;

namespace CAT_web.Controllers.ApiControllers
{
    [ApiController]
    [Route("[controller]")]
    public class EditorApiController : ControllerBase
    {
        private readonly CAT_webContext _context;
        private readonly IConfiguration _configuration;

        public EditorApiController(CAT_webContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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
                if (job == null)
                {
                    return NotFound();
                }

                var editorData = new
                {
                    translationUnits = new[]
                    {
                        new { source = "Celestial Print Velour Sleepsuit and Hat Set", target = "Lot de combinaison et chapeau en velours à imprimé céleste" },
                        new { source = "This velour set may be the star of their cosy collection!", target = "Cet ensemble en velours est peut-être la star de leur collection cosy !" },
                        new { source = "Harry Potter™ Gryffindor Phone Case", target = "Coque de téléphone Harry Potter™ Gryffondor" },
                        new { source = "Put your house pride on full display with this case", target = "Mettez la fierté de votre maison à l'honneur avec cet étui" },
                        new { source = "Disney’s Minnie Mouse Rain Jacket", target = "Veste de pluie Disney Minnie Mouse" },
                        new { source = "Their rainy day adventures just got a lot more exciting with this hooded Disney raincoat!", target = "Their rainy day adventures just got a lot more exciting with this hooded Disney raincoat!" },
                        new { source = "Coats & Jackets", target = "Coats & Jackets" },
                        new { source = "Mickey, Minnie & Friends", target = "Mickey, Minnie & Friends" },
                        new { source = "SHELL-100% Polyester with Polyurethane Coating, LINING-100% Polyester", target = "EXTÉRIEUR : 100 % polyester enduit de polyuréthane, DOUBLURE : 100 % polyester" },
                        new { source = "Disney’s Mickey Mouse and Minnie Mouse Tweezer Set", target = "Disney’s Mickey Mouse and Minnie Mouse Tweezer Set" },
                        new { source = "Mickey Mouse, Minnie Mouse", target = "Mickey Mouse, Minnie Mouse" },
                        new { source = "A baby tee that makes a statement!", target = "A baby tee that makes a statement!" },
                        new { source = "Harry Potter, Ron Weasley, Hermione Granger", target = "Harry Potter, Ron Weasley, Hermione Granger" },
                        new { source = "80% Polyester, 17% Viscose, 3% Elastane", target = "80% Polyester, 17% Viscose, 3% Elastane" },
                        new { source = "Disney's Winnie The Pooh Phone Case", target = "Étui pour téléphone Winnie l'ourson de Disney" },
                        new { source = "Wrap your device in whimsy with this case", target = "Enveloppez votre appareil de fantaisie avec cet étui" }
                    }
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
