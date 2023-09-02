using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CAT.Controllers.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MonitoringController : ControllerBase
    {
        public MonitoringController() { }

        [HttpGet]
        public async Task<IActionResult> GetMonitoringData() 
        {
            var monitoringData = new { };
                //... provide the data here


            return Ok(monitoringData);
        }
    }
}
