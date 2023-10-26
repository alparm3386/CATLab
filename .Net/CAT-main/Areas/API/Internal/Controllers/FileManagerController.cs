using Microsoft.AspNetCore.Mvc;

namespace CAT.Areas.API.Internal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileManagerController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public FileManagerController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("list")]
        public IActionResult ListFilesAndDirectories(string directoryPath = "")
        {
            try
            {
                var mountPath = _configuration["MountPath"]!.ToString();
                directoryPath = Path.Combine(mountPath, directoryPath);

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
                var mountPath = _configuration["MountPath"]!.ToString();
                directoryPath = Path.Combine(mountPath, directoryPath);
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

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
                var mountPath = _configuration["MountPath"]!.ToString();
                directoryPath = Path.Combine(mountPath, directoryPath);
                if (!Directory.Exists(directoryPath))
                    Directory.Delete(directoryPath);

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
                var mountPath = _configuration["MountPath"]!.ToString();

                // Combine the old and new paths with the directory names
                string sourcePath = Path.Combine(mountPath, oldPath);
                string targetPath = Path.Combine(mountPath, newPath);

                // Rename the directory
                Directory.Move(sourcePath, targetPath);
                
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("upload-file")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file, [FromForm] string directoryPath)
        {
            try
            {
                var mountPath = _configuration["MountPath"]!.ToString();
                directoryPath = Path.Combine(mountPath, directoryPath);

                // Check if the directoryPath is provided and not empty
                if (string.IsNullOrWhiteSpace(directoryPath))
                {
                    return BadRequest("Directory path is required.");
                }

                // Combine the directory path with the file name to create the full path
                string filePath = Path.Combine(directoryPath, file.FileName);

                // Ensure the directory exists; create it if it doesn't
                Directory.CreateDirectory(directoryPath);

                // Create or overwrite the file with the uploaded content
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return Ok($"File '{file.FileName}' uploaded successfully to '{directoryPath}'.");
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
                var mountPath = _configuration["MountPath"]!.ToString();
                oldPath = Path.Combine(mountPath, oldPath);
                newPath = Path.Combine(mountPath, newPath);
                // Check if the oldPath and newPath are provided and not empty
                if (string.IsNullOrWhiteSpace(oldPath) || string.IsNullOrWhiteSpace(newPath))
                {
                    return BadRequest("Both oldPath and newPath are required.");
                }

                // Rename the file
                System.IO.File.Move(oldPath, newPath);

                return Ok($"File renamed from '{oldPath}' to '{newPath}'.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpDelete("delete-file")]
        public IActionResult DeleteFile(string filePath)
        {
            try
            {
                var mountPath = _configuration["MountPath"]!.ToString();
                filePath = Path.Combine(mountPath, filePath);
                // Check if the filePath is provided and not empty
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    return BadRequest("File path is required.");
                }

                // Check if the file exists
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound($"File not found at '{filePath}'.");
                }

                // Delete the file
                System.IO.File.Delete(filePath);

                return Ok($"File at '{filePath}' has been deleted.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
