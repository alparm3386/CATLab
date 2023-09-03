using AutoMapper;
using CAT.Areas.Identity.Data;
using CAT.Data;
using CAT.Services.Common;
using Microsoft.AspNetCore.Mvc;
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
        private readonly JobService _jobService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public EditorApiController(IConfiguration configuration,
            JobService jobService, IMapper mapper, ILogger<JobService> logger)
        {
            _configuration = configuration;
            _jobService = jobService;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet("DownloadDocument/{idJob}")]
        public async Task<IActionResult> DownloadDocument(int idJob)
        {
            // Offload the execution of CreateDocument to a separate thread.
            var fileData = await Task.Run(() => _jobService.CreateDocument(idJob));

            return File(fileData.Content!, "application/octet-stream", fileData.FileName);  // Change the MIME type if you know the specific type for the file
        }
    }
}
