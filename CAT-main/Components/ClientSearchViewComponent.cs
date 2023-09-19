using Microsoft.AspNetCore.Mvc;

using CAT.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CAT.Components
{
    public class ClientSearchViewComponent : ViewComponent
    {
        private readonly MainDbContext _mainDbContext;

        public ClientSearchViewComponent(MainDbContext mainDbContext)
        {
            _mainDbContext = mainDbContext;
        }

        public async Task<IViewComponentResult> InvokeAsync(string term = "")
        {
            var suggestions = await _mainDbContext.Clients.Include(c => c.Company)
                .Where(item => item.Company.Name.Contains(term))
                .Take(20)
                .ToListAsync();

            return View(suggestions);
        }
    }
}
