using AutoMapper;
using CAT.Areas.Identity.Data;
using CAT.Data;
using CAT.Services.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CAT.Areas.API.Internal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EditorApiController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IJobService _jobService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public EditorApiController(IConfiguration configuration,
            IJobService jobService, IMapper mapper, ILogger<JobService> logger)
        {
            _configuration = configuration;
            _jobService = jobService;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet("DownloadDocument/{jobId}")]
        public IActionResult DownloadDocument(int jobId)
        {
            // Offload the execution of CreateDocument to a separate thread.
            //var fileData = await Task.Run(() => _jobService.CreateDocument(jobId));
            try
            {
                var userId = "tmp001";
                var fileData = _jobService.CreateDocument(jobId, userId, false);

                return File(fileData.Content!, "application/octet-stream", fileData.FileName);  // Change the MIME type if you know the specific type for the file
            }
            catch (Exception ex)
            {
                _logger.LogError("DownloadDocument error -> jobId: " + jobId + " " + ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
