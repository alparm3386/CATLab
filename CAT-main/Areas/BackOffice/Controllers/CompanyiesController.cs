using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CAT.Areas.BackOffice.Models.ViewModels;
using CAT.Data;

namespace CAT.Areas.BackOffice.Controllers
{
    [Area("BackOffice")]
    public class CompaniesController : Controller
    {
        private readonly MainDbContext _context;

        public CompaniesController(MainDbContext context)
        {
            _context = context;
        }

        // GET: BackOffice/CompanyViewModels
        public async Task<IActionResult> Index()
        {
            //return _context.CompanyViewModel != null ? 
            //            View(await _context.CompanyViewModel.ToListAsync()) :
            //            Problem("Entity set 'MainDbContext.CompanyViewModel'  is null.");
            return View();
        }

        // GET: BackOffice/CompanyViewModels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            return View();
        }

        // GET: BackOffice/CompanyViewModels/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BackOffice/CompanyViewModels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CompanyGroupId,Name,AddressId,Line1,Line2,City,PostalCode,Country,Region,Phone")] CompanyViewModel companyViewModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(companyViewModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(companyViewModel);
        }

        // GET: BackOffice/CompanyViewModels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            return View();
        }

        // POST: BackOffice/CompanyViewModels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyGroupId,Name,AddressId,Line1,Line2,City,PostalCode,Country,Region,Phone")] CompanyViewModel companyViewModel)
        {
            if (id != companyViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(companyViewModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyViewModelExists(companyViewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(companyViewModel);
        }

        // GET: BackOffice/CompanyViewModels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            throw new InvalidOperationException("This operation cannot be performed in the object's current state.");

            //return View(companyViewModel);
        }

        // POST: BackOffice/CompanyViewModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            throw new InvalidOperationException("This operation cannot be performed in the object's current state.");
            //return RedirectToAction(nameof(Index));
        }

        private bool CompanyViewModelExists(int id)
        {
            return false; // (_context.CompanyViewModel?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
