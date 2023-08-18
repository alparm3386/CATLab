using AutoMapper;
using CAT.Areas.Identity.Data;
using CAT.Data;
using CAT.Services.CAT;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace CAT.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class EditorApiController : ControllerBase
    {
        private readonly IdentityDbContext _identityDBContext;
        private readonly MainDbContext _mainDbContext;
        private readonly TranslationUnitsDbContext _translationUnitsDbContext;
        private readonly IConfiguration _configuration;
        private readonly JobService _jobService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public EditorApiController(IdentityDbContext identityDBContext, MainDbContext mainDbContext, TranslationUnitsDbContext translationUnitsDbContext,
            IConfiguration configuration, JobService jobService, IMapper mapper, ILogger<JobService> logger)
        {
            _identityDBContext = identityDBContext;
            _mainDbContext = mainDbContext;
            _translationUnitsDbContext = translationUnitsDbContext;
            _configuration = configuration;
            _jobService = jobService;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet("DownloadDocument/{id}")]
        public async Task<IActionResult> DownloadDocument(int id)
        {
            var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id);

            if (document == null)
                return NotFound("Document not found.");

            return File(document.Data, "application/octet-stream", document.FileName);  // Change the MIME type if you know the specific type for the file
        }
    }
}
