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
        private readonly MainDbContext _mainDbContext;

        public TestingPageModel(MainDbContext mainDbContext)
        {
            _mainDbContext = mainDbContext;
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
          if (!ModelState.IsValid || _mainDbContext.Jobs == null || Job == null)
            {
                return Page();
            }

            _mainDbContext.Jobs.Add(Job);
            await _mainDbContext.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
