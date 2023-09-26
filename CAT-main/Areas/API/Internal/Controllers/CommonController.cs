using CAT.Data;
using CAT.Models.Entities.Main;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CAT.Areas.API.Internal.Controllers
{
    [Area("API")]
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Policy = "AdminsOnly")]
    public class CommonController : ControllerBase
    {
        private const int AUTOCOMPLETE_LIMIT = 15;
        private readonly DbContextContainer _dbContextContainer;

        public CommonController(DbContextContainer dbContextContainer)
        {
            _dbContextContainer = dbContextContainer;
        }

        [HttpGet("GetFilteredClients")]
        public async Task<IActionResult> GetFilteredClients(string term, int? limit)
        {
            limit = limit ?? AUTOCOMPLETE_LIMIT;
            // Query the database based on "term". 
            var clients = await _dbContextContainer.MainContext.Clients.AsNoTracking().Include(c => c.Company)
                .Where(item => item.Company.Name.Contains(term))
                .Take((int)limit)
                .ToListAsync();

            //join into the users table 
            clients = (from client in clients
                             join user in _dbContextContainer.IdentityContext.Users on client.UserId equals user.Id
                             select new Client
                             {
                                 Id = client.Id,
                                 Company = client.Company,
                                 CompanyId = client.CompanyId,
                                 User = user
                             }).ToList();

            return Ok(clients);
        }

        [HttpGet("GetFilteredLinguists")]
        public async Task<IActionResult> GetFilteredLinguists(string term, int? limit)
        {
            limit = limit ?? AUTOCOMPLETE_LIMIT;

            // Optimize by pushing filtering and joining to the database query
            var linguists = await (from linguist in _dbContextContainer.MainContext.Linguists
                                   join user in _dbContextContainer.IdentityContext.Users
                                   on linguist.UserId equals user.Id
                                   where (user.FirstName + " " + user.LastName).Contains(term)
                                   select new Linguist
                                   {
                                       UserId = user.Id,
                                       User = user
                                   })
                                   .Take(limit.Value)
                                   .AsNoTracking()
                                   .ToListAsync();

            return Ok(linguists);
        }
    }
}
