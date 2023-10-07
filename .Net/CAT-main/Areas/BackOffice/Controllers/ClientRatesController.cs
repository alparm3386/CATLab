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
using CAT.Infrastructure;

namespace CAT.Areas.BackOffice.Controllers
{
    [Area("BackOffice")]
    public class ClientRatesController : Controller
    {
        private readonly MainDbContext _context;

        public ClientRatesController(MainDbContext context)
        {
            _context = context;
        }

        // GET: BackOffice/Rates
        public async Task<IActionResult> Index(int? companyId, int? sourceLanguageFilter, int? targetLanguageFilter,
            int? specialityFilter, int? taskFilter, int? pageNumber)
        {
            try
            {
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

                //get the company
                var company = _context.Companies.Where(c => c.Id == companyId).AsNoTracking().FirstOrDefault();
                if (company == null)
                    throw new CATException("Invalid company");
                ViewData["CompanyId"] = company.Id;
                ViewData["CompanyName"] = company.Name;

                //get the rates
                var ratesQuery = _context.ClientRates.AsNoTracking().Include(cr => cr.Rate)
                    .Where(cr => cr.CompanyId == companyId);
                if (sourceLanguageFilter.HasValue && sourceLanguageFilter > 0)
                    ratesQuery = ratesQuery.Where(cr => cr.Rate.SourceLanguageId == sourceLanguageFilter.Value);
                if (targetLanguageFilter.HasValue && targetLanguageFilter > 0)
                    ratesQuery = ratesQuery.Where(cr => cr.Rate.TargetLanguageId == targetLanguageFilter.Value);
                if (specialityFilter.HasValue && specialityFilter > 0)
                    ratesQuery = ratesQuery.Where(cr => cr.Rate.Speciality == specialityFilter.Value);
                if (taskFilter.HasValue && taskFilter > 0)
                    ratesQuery = ratesQuery.Where(cr => cr.Rate.Task == taskFilter.Value);
                //var rates = await ratesQuery.ToListAsync();
                int pageSize = 10;
                pageNumber = pageNumber ?? 1;
                var paginatedRates = await PaginatedList<ClientRate>.CreateAsync(ratesQuery, (int)pageNumber, pageSize);
                return View(paginatedRates);
            }
            catch (Exception ex)
            {
                // set error message here that is displayed in the view
                if (ex is CATException)
                    ViewData["ErrorMessage"] = ex.Message;
                else
                    ViewData["ErrorMessage"] = "There was an error processing your request. Please try again later.";
                // Optionally log the error: _logger.LogError(ex, "Error message here");
            }

            return View(new PaginatedList<ClientRate>(new List<ClientRate>(), 0, 0, 1));
        }

        // GET: BackOffice/Rates/Create
        public async Task<IActionResult> Create(int? companyId)
        {
            try
            {
                //set the error message is there was problem in the previous request.
                if (TempData["ErrorMessage"] != null)
                    ViewData["ErrorMessage"] = TempData["ErrorMessage"]!.ToString();

                //get the company
                var company = _context.Companies.Where(c => c.Id == companyId).AsNoTracking().FirstOrDefault();
                if (company == null)
                    throw new CATException("Invalid company");
                ViewData["CompanyId"] = company.Id;
                ViewData["CompanyName"] = company.Name;
            }
            catch (Exception ex)
            {
                // set error message here that is displayed in the view
                if (ex is CATException)
                    ViewData["ErrorMessage"] = ex.Message;
                else
                    ViewData["ErrorMessage"] = "There was an error processing your request. Please try again later.";
                // Optionally log the error: _logger.LogError(ex, "Error message here");
            }

            var languages = await _context.Languages.ToDictionaryAsync(l => l.Id, l => l.Name);
            ViewData["Languages"] = new SelectList(languages, "Key", "Value");
            ViewData["Specialities"] = new SelectList(EnumHelper.EnumToDisplayNamesDictionary<Speciality>(), "Key", "Value");

            return View();
        }

        // POST: BackOffice/Rates/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? companyId, ClientRate clientRate)
        {
            try
            {
                if (!companyId.HasValue)
                    throw new CATException("Invalid company");

                //get the base rate
                var rate = await _context.Rates.Where(cr => cr.SourceLanguageId == clientRate.Rate.SourceLanguageId &&
                    cr.TargetLanguageId == clientRate.Rate.TargetLanguageId &&
                    cr.Speciality == clientRate.Rate.Speciality &&
                    cr.Task == clientRate.Rate.Task).AsNoTracking().FirstOrDefaultAsync();

                if (rate == null)
                    throw new CATException("Base rate not found");

                ModelState.Remove("Company");
                ModelState.Remove("Rate");
                if (ModelState.IsValid)
                {
                    clientRate.RateId = rate.Id;
                    clientRate.Rate = null!;
                    _context.Add(clientRate);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index), new { companyId });
                }
                else
                    throw new CATException("Invalid model state");
            }
            catch (Exception ex)
            {
                // set error message here that is displayed in the view
                string error = ex.Message;
                if (ex is not CATException)
                    error = "There was an error processing your request. Please try again later.";
                // Optionally log the error: _logger.LogError(ex, "Error message here");
                TempData["ErrorMessage"] = error;
                return RedirectToAction(nameof(Create), new { companyId });
            }
        }

        // GET: BackOffice/Rates/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.ClientRates == null)
            {
                return NotFound();
            }

            var clientRate = await _context.ClientRates.Include(cr => cr.Rate).Where(cr => cr.Id == id).AsNoTracking().FirstOrDefaultAsync();
            if (clientRate == null)
            {
                return NotFound();
            }

            //the languages
            ViewData["Languages"] = await _context.Languages.ToDictionaryAsync(l => l.Id, l => l.Name);
            ViewData["Specialities"] = EnumHelper.EnumToDisplayNamesDictionary<Speciality>();
            ViewData["Tasks"] = EnumHelper.EnumToDisplayNamesDictionary<Task>();

            return View(clientRate);
        }

        // POST: BackOffice/Rates/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RateToClient")] ClientRate clientRate)
        {
            if (id != clientRate.Id)
            {
                return NotFound();
            }

            //update the stored rate
            var storedClientRate = await _context.ClientRates.Include(cr => cr.Rate).Where(cr => cr.Id == id).AsNoTracking().FirstOrDefaultAsync();
            storedClientRate!.RateToClient = clientRate.RateToClient;

            ModelState.Remove("Company");
            ModelState.Remove("Rate");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(storedClientRate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return RedirectToAction(nameof(Index), new { companyId = storedClientRate.CompanyId });
            }
            return View(clientRate);
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
    }
}
