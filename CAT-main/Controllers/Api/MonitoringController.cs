using CAT.Services.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CAT.Controllers.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
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
    }
}
