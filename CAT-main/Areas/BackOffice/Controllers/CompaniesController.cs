using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CAT.Models.Entities.Main;
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

        // GET: BackOffice/Companies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Companies.Include(c => c.Address).ToListAsync());
        }

        // GET: BackOffice/Companies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Companies == null)
                return NotFound();

            var company = await _context.Companies
                .Include(c => c.Address)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (company == null)
                return NotFound();

            return View(company);
        }

        // GET: BackOffice/Companies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BackOffice/Companies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Address")] Company company)
        {
            if (ModelState.IsValid)
            {
                _context.Add(company);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }

        // GET: BackOffice/Companies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Companies == null)
                return NotFound();

            var company = await _context.Companies
                .Include(c => c.Address)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (company == null)
                return NotFound();

            return View(company);
        }

        // POST: BackOffice/Companies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address")] Company company)
        {
            if (id != company.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(company);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyExists(company.Id))
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
            return View(company);
        }

        // GET: BackOffice/Companies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            throw new InvalidOperationException("This operation cannot be performed in the object's current state.");

            //return View(Companies);
        }

        // POST: BackOffice/Companies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            throw new InvalidOperationException("This operation cannot be performed in the object's current state.");
            //return RedirectToAction(nameof(Index));
        }

        private bool CompanyExists(int id)
        {
            return false; // (_context.Companies?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
