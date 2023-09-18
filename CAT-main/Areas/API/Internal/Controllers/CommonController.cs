using CAT.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CAT.Areas.API.Internal.Controllers
{
    [Area("API")]
    [Route("api/{controller}/{action}")]
    [ApiController]
    //[Authorize(Policy = "AdminsOnly")]
    public class CommonController : ControllerBase
    {
        private readonly MainDbContext _mainDbContext;

        public CommonController(MainDbContext mainDbContext)
        {
            _mainDbContext = mainDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetClientSuggestions(string term)
        {
            // Query the database based on "term". 
            var suggestions = await _mainDbContext.Clients.Include(c => c.Company)
                .Where(item => item.Company.Name.Contains(term))
                .Take(10)
                .ToListAsync();

            return Ok(suggestions);
        }
    }
}
