using CAT.Areas.BackOffice.Services;
using CAT.Infrastructure;
using CAT.Models.Entities.Main;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        public class AllocationParams
        {
            public int JobId { get; set; }
            public int Task { get; set; }
            public string UserId { get; set; } = default!;
        }
        [HttpPost("AllocateJob")]
        public async Task<IActionResult> AllocateJob([FromBody] AllocationParams allocationParams)
        {
            try
            {
                await _monitoringService.AllocatJob(allocationParams.JobId, (Enums.Task)allocationParams.Task, allocationParams.UserId);

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
