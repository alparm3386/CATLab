using CAT.Data;
using CAT.Models.Entities.Main;
using CAT.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CAT.Areas.API.Internal.Controllers
{
    [Area("API")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminsOnly")]
    public class CommonController : ControllerBase
    {
        private const int AUTOCOMPLETE_LIMIT = 15;
        private readonly DbContextContainer _dbContextContainer;
        private readonly IConfiguration _configuration;
        private readonly ICatClientFactory _catClientFactory;

        public CommonController(DbContextContainer dbContextContainer, IConfiguration configuration, ICatClientFactory catClientFactory)
        {
            _dbContextContainer = dbContextContainer;
            _configuration = configuration;
            _catClientFactory = catClientFactory;
        }

        [HttpGet("GetFilteredClients")]
        public async Task<IActionResult> GetFilteredClients(string term, int? limit)
        {
            limit ??= AUTOCOMPLETE_LIMIT;
            // Query the database based on "term". 
            var clients = await _dbContextContainer.MainContext.Clients.AsNoTracking().Include(c => c.Company)
                .Where(item => item.Company.Name.Contains(term))
                .Take((int)limit)
                .ToListAsync();

            //join into the users table 
            clients = (from client in clients
                             join user in _dbContextContainer.IdentityContext.Users on client.UserId equals user.Id
                             select new Client
                             {
                                 Id = client.Id,
                                 Company = client.Company,
                                 CompanyId = client.CompanyId,
                                 User = user
                             }).ToList();

            return Ok(clients);
        }

        [HttpGet("GetLinguists")]
        public async Task<IActionResult> GetLinguists(int sourceLanguageId, int targetLanguageId, 
            int speciality, int task)
        {
            try
            {
                //get the linguists
                var linguists = await _dbContextContainer.MainContext.Linguists.AsNoTracking()
                    .Include(l => l.LinguistRates)
                    .ThenInclude(lr => lr.Rate)
                    .Where(l => l.LinguistRates.Any(lr => lr.Rate.SourceLanguageId == sourceLanguageId &&
                        lr.Rate.TargetLanguageId == targetLanguageId && lr.Rate.Speciality == speciality && lr.Rate.Task == task))
                    .ToListAsync();

                //join into the users table 
                linguists = (from linguist in linguists
                             join user in _dbContextContainer.IdentityContext.Users on linguist.UserId equals user.Id
                             select new Linguist
                             {
                                 Id = linguist.Id,
                                 UserId = linguist.UserId,
                                 User = user
                             }).ToList();

                return Ok(linguists);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [Route("GetProfilePicture/{userId}")]
        [HttpGet]
        public IActionResult GetProfilePicture(string userId)
        {
            try
            {
                string mimeType = "image/jpeg";
                var avatarFolder = _configuration["Avatar"];
                var imagePath = Path.Combine(avatarFolder!, userId + ".jpeg");
                if (!System.IO.File.Exists(imagePath))
                    imagePath = Path.Combine(avatarFolder!, "default.jpeg");
                var imageBytes = System.IO.File.ReadAllBytes(imagePath);
                return new FileContentResult(imageBytes, mimeType);
            }
            catch (Exception)
            {
                return Problem("System error");
            }
        }

        [Route("ReloadConfig")]
        [HttpGet]
        public IActionResult ReloadConfig()
        {
            try
            {
                // Retrieve the DatabaseConfigurationProvider instance from the IConfiguration
                var configurationRoot = _configuration as IConfigurationRoot;
                var databaseConfigProvider = (Configuration.DatabaseConfigurationProvider)configurationRoot!.Providers
                    .First(provider => provider is Configuration.DatabaseConfigurationProvider);

                // Reload configuration from the database
                databaseConfigProvider.Reload();

                //force recreating the grpc channel
                _catClientFactory.ResetChannel();

                return Ok("Reloaded");
            }
            catch (Exception ex)
            {
                return Problem("System error -> " + ex.Message);
            }
        }
    }
}
