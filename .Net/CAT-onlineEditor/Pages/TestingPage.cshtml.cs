using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CAT.Data;
using CAT.Models.Entities;
using CAT.Models.Entities.Main;

namespace CAT.Pages
{
    public class TestingPageModel : PageModel
    {
        private readonly DbContextContainer _dbContextContainer;

        public TestingPageModel(DbContextContainer dbContextContainer)
        {
            _dbContextContainer = dbContextContainer;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Job Job { get; set; } = default!;
        

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
          if (!ModelState.IsValid || _dbContextContainer.MainContext.Jobs == null || Job == null)
            {
                return Page();
            }

            _dbContextContainer.MainContext.Jobs.Add(Job);
            await _dbContextContainer.MainContext.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
