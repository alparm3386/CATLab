using CAT.Areas.BackOffice.Services;
using CAT.Infrastructure;
using CAT.Models.Entities.Main;
using CAT.Services.Common;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CAT.Areas.API.Internal.Controllers
{
    [Area("API")]
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Policy = "AdminsOnly")]
    public class MonitoringController : ControllerBase
    {
        private IMonitoringService _monitoringService;
        private readonly IDocumentService _documentService;

        public MonitoringController(IMonitoringService monitoringService, IDocumentService documentService)
        {
            _monitoringService = monitoringService;
            _documentService = documentService;
        }

        [HttpGet("GetMonitoringData")]
        public async Task<IActionResult> GetMonitoringData(DateTime? dateFrom, DateTime? dateTo)
        {
            dateFrom = dateFrom ?? DateTime.MinValue;
            dateTo = dateTo ?? DateTime.MaxValue;

            var monitoringData = await _monitoringService.GetMonitoringData((DateTime)dateFrom, (DateTime)dateTo);

            return Ok(monitoringData);
        }

        [System.ComponentModel.DisplayName("MethodWithParameters yyy {0}")]
        public void MethodWithParameters(string message)
        {
            Console.WriteLine(message);
        }

        [HttpGet("GetJobData")]
        public async Task<IActionResult> GetJobData(int jobId)
        {
            var jobData = await _monitoringService.GetJobData(jobId);
            return Ok(jobData);
        }

        [HttpPost("AllocateJob")]
        public async Task<IActionResult> AllocateJob(int jobId, int task, string userId)
        {
            try
            {
                var allocatorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (allocatorUserId == null)
                    throw new Exception("Invalid user.");

                if (userId == "0")
                    userId = allocatorUserId;

                await _monitoringService.AllocateJob(jobId, (Enums.Task)task, userId, allocatorUserId);

                return Ok(new { Message = "Allocated" });
            }
            catch (Exception ex)
            {
                if (ex is CATException)
                    return Problem(ex.Message);

                return Problem("Server error");
            }
        }

        [HttpPost("DeallocateJob")]
        public async Task<IActionResult> DeallocateJob(int jobId, int task, string deallocationReason)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                    userId = new Guid().ToString();

                await _monitoringService.DeallocateJob(jobId, userId, (Enums.Task)task, deallocationReason);

                return Ok(new { Message = "Allocated" });
            }
            catch (Exception ex)
            {
                if (ex is CATException)
                    return Problem(ex.Message);

                return Problem("Server error");
            }
        }

        [HttpGet("DownloadDocument/{id}")]
        public async Task<IActionResult> DownloadDocument(int id)
        {
            // Define the file path and name on the server
            string filePath = await _documentService.GetDocumentFolderAsync(id);

            // Check if the file exists
            if (!System.IO.File.Exists(filePath))
                return NotFound(); // Return a 404 Not Found response if the file does not exist

            // Get the file's content type
            string contentType = "application/octet-stream"; // Set the appropriate content type for your file

            // Define the file download response
            var fileContentResult = new FileContentResult(System.IO.File.ReadAllBytes(filePath), contentType)
            {
                FileDownloadName = Path.GetFileName(filePath)
            };

            return fileContentResult;
        }
    }
}
