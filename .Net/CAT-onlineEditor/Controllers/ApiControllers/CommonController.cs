using CAT.Data;
using CAT.Models.Entities.Main;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CAT.Areas.API.Internal.Controllers
{
    [Area("API")]
    [Route("onlineeditor/api/[controller]")]
    [ApiController]
    //[Authorize(Policy = "AdminsOnly")]
    public class CommonController : ControllerBase
    {
        private readonly DbContextContainer _dbContextContainer;
        private readonly IConfiguration _configuration;

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

                return Ok("Reloaded");
            }
            catch (Exception ex)
            {
                return Problem("System error -> " + ex.Message);
            }
        }
    }
}
