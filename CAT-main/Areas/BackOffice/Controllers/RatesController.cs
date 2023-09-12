using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CAT.Data;
using CAT.Models.Entities.Main;

namespace CAT.Areas.BackOffice.Controllers
{
    [Area("BackOffice")]
    public class RatesController : Controller
    {
        private readonly MainDbContext _context;

        public RatesController(MainDbContext context)
        {
            _context = context;
        }

        // GET: BackOffice/Rates
        public async Task<IActionResult> Index()
        {
            return _context.Rates != null ?
                        View(await _context.Rates.ToListAsync()) :
                        Problem("Entity set 'MainDbContext.Rates'  is null.");
        }

        // GET: BackOffice/Rates/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Rates == null)
            {
                return NotFound();
            }

            var rate = await _context.Rates
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rate == null)
            {
                return NotFound();
            }

            return View(rate);
        }

        // GET: BackOffice/Rates/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BackOffice/Rates/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,SourceLanguageId,TargetLanguageId,Speciality,Task,RateToClient,RateToTranslator")] Rate rate)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(rate);
        }

        // GET: BackOffice/Rates/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Rates == null)
            {
                return NotFound();
            }

            var rate = await _context.Rates.FindAsync(id);
            if (rate == null)
            {
                return NotFound();
            }
            return View(rate);
        }

        // POST: BackOffice/Rates/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SourceLanguageId,TargetLanguageId,Speciality,Task,RateToClient,RateToTranslator")] Rate rate)
        {
            if (id != rate.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RateExists(rate.Id))
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
            return View(rate);
        }

        // GET: BackOffice/Rates/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Rates == null)
            {
                return NotFound();
            }

            var rate = await _context.Rates
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rate == null)
            {
                return NotFound();
            }

            return View(rate);
        }

        // POST: BackOffice/Rates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Rates == null)
            {
                return Problem("Entity set 'MainDbContext.Rates'  is null.");
            }
            var rate = await _context.Rates.FindAsync(id);
            if (rate != null)
            {
                _context.Rates.Remove(rate);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RateExists(int id)
        {
            return (_context.Rates?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
