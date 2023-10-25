using CAT.Areas.BackOffice.Services;
using CAT.Infrastructure;
using CAT.Models.Entities.Main;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Lucene.Net.Util.Fst.Util;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CAT.Areas.API.Internal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileManagerController : ControllerBase
    {

        [HttpGet("list")]
        public IActionResult ListFilesAndDirectories(string directoryPath)
        {
            try
            {
                // Implement logic to list files and directories in the specified directoryPath.
                var directoryInfo = new DirectoryInfo(directoryPath);

                if (!directoryInfo.Exists)
                {
                    return NotFound($"Directory not found: {directoryPath}");
                }

                var files = directoryInfo.GetFiles().Select(file => new
                {
                    Name = file.Name,
                    SizeInBytes = file.Length,
                    LastModified = file.LastWriteTimeUtc
                });

                var directories = directoryInfo.GetDirectories().Select(directory => new
                {
                    Name = directory.Name,
                    LastModified = directory.LastWriteTimeUtc
                });

                var result = new
                {
                    Files = files,
                    Directories = directories
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("create-directory")]
        public IActionResult CreateDirectory(string directoryPath)
        {
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpDelete("delete-directory")]
        public IActionResult DeleteDirectory(string directoryPath)
        {
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPut("rename-directory")]
        public IActionResult RenameDirectory(string oldPath, string newPath)
        {
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("upload-file")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file, string directoryPath)
        {
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPut("rename-file")]
        public IActionResult RenameFile(string oldPath, string newPath)
        {
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
