using CAT.Areas.BackOffice.Services;
using CAT.Infrastructure;
using CAT.Models.Entities.Main;
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

        public MonitoringController(IMonitoringService monitoringService)
        {
            _monitoringService = monitoringService;
        }

        [HttpGet("GetMonitoringData")]
        public async Task<IActionResult> GetMonitoringData(DateTime? dateFrom, DateTime? dateTo)
        {
            dateFrom = dateFrom ?? DateTime.MinValue;
            dateTo = dateTo ?? DateTime.MaxValue;

            var monitoringData = await _monitoringService.GetMonitoringData((DateTime)dateFrom, (DateTime)dateTo);

            return Ok(monitoringData);
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
                    allocatorUserId = new Guid().ToString();
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
    }
}
