using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CAT.Data;
using CAT.Models.Entities.Main;
using CAT.Helpers;
using CAT.Enums;
using Task = CAT.Enums.Task;

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
        public async Task<IActionResult> Index(int? sourceLanguageFilter, int? targetLanguageFilter, 
            int? specialityFilter, int? taskFilter, int? pageNumber)
        {
            try
            {
                //get the rates
                var ratesQuery = _context.Rates.AsNoTracking();
                if (sourceLanguageFilter.HasValue && sourceLanguageFilter > 0)
                    ratesQuery = ratesQuery.Where(r => r.SourceLanguageId == sourceLanguageFilter.Value);
                if (targetLanguageFilter.HasValue && targetLanguageFilter > 0)
                    ratesQuery = ratesQuery.Where(r => r.TargetLanguageId == targetLanguageFilter.Value);
                if (specialityFilter.HasValue && specialityFilter > 0)
                    ratesQuery = ratesQuery.Where(r => r.Speciality == specialityFilter.Value);
                if (taskFilter.HasValue && taskFilter > 0)
                    ratesQuery = ratesQuery.Where(r => r.Task == taskFilter.Value);
                //var rates = await ratesQuery.ToListAsync();

                //set the lists
                var languages = await _context.Languages.ToDictionaryAsync(l => l.Id, l => l.Name);
                languages = new Dictionary<int, string> { { -1, "Select all" } }.Concat(languages).ToDictionary(pair => pair.Key, pair => pair.Value);
                ViewData["LanguagesSelectList"] = new SelectList(languages, "Key", "Value");

                var specialities = EnumHelper.EnumToDisplayNamesDictionary<Speciality>();
                specialities = new Dictionary<int, string> { { -1, "Select all" } }.Concat(specialities).ToDictionary(pair => pair.Key, pair => pair.Value);
                ViewData["SpecialitiesSelectList"] = new SelectList(specialities, "Key", "Value");

                ViewData["Specialities"] = EnumHelper.EnumToDisplayNamesDictionary<Speciality>();
                ViewData["Tasks"] = EnumHelper.EnumToDisplayNamesDictionary<Enums.Task>();

                //the filters
                ViewData["SourceLanguageFilter"] = sourceLanguageFilter ?? -1;
                ViewData["TargetLanguageFilter"] = targetLanguageFilter ?? -1;
                ViewData["specialityFilter"] = specialityFilter ?? -1;
                ViewData["taskFilter"] = taskFilter ?? -1;

                int pageSize = 10;
                pageNumber = pageNumber ?? 1;
                var paginatedRates = await PaginatedList<Rate>.CreateAsync(ratesQuery, (int)pageNumber, pageSize);
                return View(paginatedRates);
            }
            catch (Exception ex)
            {
                // set error message here that is displayed in the view
                //ViewData["ErrorMessage"] = "There was an error processing your request. Please try again later.";
                ViewData["ErrorMessage"] = ex.Message;
                // Optionally log the error: _logger.LogError(ex, "Error message here");
            }

            return View(new List<Rate>());
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

            //the languages
            ViewData["Languages"] = await _context.Languages.ToDictionaryAsync(l => l.Id, l => l.Name);
            ViewData["Specialities"] = EnumHelper.EnumToDisplayNamesDictionary<Speciality>();
            ViewData["Tasks"] = EnumHelper.EnumToDisplayNamesDictionary<Task>();

            return View(rate);
        }

        // POST: BackOffice/Rates/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RateToClient,RateToTranslator")] Rate rate)
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
