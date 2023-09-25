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
    public class LinguistRatesController : Controller
    {
        private readonly DbContextContainer _contextContainer;

        public LinguistRatesController(DbContextContainer contextContainer)
        {
            _contextContainer = contextContainer;
        }

        // GET: BackOffice/Rates
        public async Task<IActionResult> Index(int? linguistId, int? sourceLanguageFilter, int? targetLanguageFilter,
            int? specialityFilter, int? taskFilter, int? pageNumber)
        {
            try
            {
                //set the lists
                var languages = await _contextContainer.MainContext.Languages.ToDictionaryAsync(l => l.Id, l => l.Name);
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

                //get the linguist
                var linguist = _contextContainer.MainContext.Linguists.Where(c => c.Id == linguistId).AsNoTracking().FirstOrDefault();
                if (linguist == null)
                    throw new CATException("Invalid linguist");

                //get the user
                var user = await _contextContainer.IdentityContext.Users.FindAsync(linguist.UserId);
                ViewData["linguistId"] = linguist.Id;
                ViewData["linguistName"] = user!.FullName;

                //get the rates
                var ratesQuery = _contextContainer.MainContext.LinguistRates.Include(cr => cr.Rate).AsNoTracking();
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
                var paginatedRates = await PaginatedList<LinguistRate>.CreateAsync(ratesQuery, (int)pageNumber, pageSize);
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

            return View(new PaginatedList<LinguistRate>(new List<LinguistRate>(), 0, 0, 1));
        }

        // GET: BackOffice/Rates/Create
        public async Task<IActionResult> Create(int? linguistId)
        {
            try
            {
                //get the linguist
                var linguist = _contextContainer.MainContext.Linguists.Where(c => c.Id == linguistId).AsNoTracking().FirstOrDefault();
                if (linguist == null)
                    throw new CATException("Invalid linguist");
                //get the user
                var user = await _contextContainer.IdentityContext.Users.FindAsync(linguist.UserId);
                if (user == null)
                    throw new CATException("Invalid user");

                ViewData["linguistId"] = linguist.Id;
                ViewData["linguistName"] = user.FullName;
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

            var languages = await _contextContainer.MainContext.Languages.ToDictionaryAsync(l => l.Id, l => l.Name);
            ViewData["Languages"] = new SelectList(languages, "Key", "Value");
            ViewData["Specialities"] = new SelectList(EnumHelper.EnumToDisplayNamesDictionary<Speciality>(), "Key", "Value");

            return View();
        }

        // POST: BackOffice/Rates/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? linguistId, LinguistRate linguistRate)
        {
            try
            {
                if (!linguistId.HasValue)
                    throw new CATException("Invalid linguist");

                //get the base rate
                var rate = await _contextContainer.MainContext.Rates.Where(cr => cr.SourceLanguageId == linguistRate.Rate.SourceLanguageId &&
                    cr.TargetLanguageId == linguistRate.Rate.TargetLanguageId &&
                    cr.Speciality == linguistRate.Rate.Speciality &&
                    cr.Task == linguistRate.Rate.Task).AsNoTracking().FirstOrDefaultAsync();

                if (rate == null)
                    throw new CATException("Base rate not found");

                ModelState.Remove("Linguist");
                ModelState.Remove("Rate");
                if (ModelState.IsValid)
                {
                    linguistRate.RateId = rate.Id;
                    linguistRate.Rate = null!;
                    _contextContainer.MainContext.Add(linguistRate);
                    await _contextContainer.MainContext.SaveChangesAsync();
                    return RedirectToAction(nameof(Index), new { linguistId });
                }
                else
                    throw new CATException("Invalid model state");
            }
            catch (Exception ex)
            {
                // set error message here that is displayed in the view
                if (ex is CATException)
                    ViewData["ErrorMessage"] = ex.Message;
                else
                    ViewData["ErrorMessage"] = "There was an error processing your request. Please try again later.";
                // Optionally log the error: _logger.LogError(ex, "Error message here");

                var languages = await _contextContainer.MainContext.Languages.ToDictionaryAsync(l => l.Id, l => l.Name);
                ViewData["Languages"] = new SelectList(languages, "Key", "Value");
                ViewData["Specialities"] = new SelectList(EnumHelper.EnumToDisplayNamesDictionary<Speciality>(), "Key", "Value");

                ViewData["linguistId"] = linguistId;
                ViewData["linguistName"] = "";

                return View();
            }
        }

        // GET: BackOffice/Rates/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _contextContainer.MainContext.LinguistRates == null)
            {
                return NotFound();
            }

            var linguistRate = await _contextContainer.MainContext.LinguistRates.Include(cr => cr.Rate).Where(cr => cr.Id == id).AsNoTracking().FirstOrDefaultAsync();
            if (linguistRate == null)
            {
                return NotFound();
            }

            //the languages
            ViewData["Languages"] = await _contextContainer.MainContext.Languages.ToDictionaryAsync(l => l.Id, l => l.Name);
            ViewData["Specialities"] = EnumHelper.EnumToDisplayNamesDictionary<Speciality>();
            ViewData["Tasks"] = EnumHelper.EnumToDisplayNamesDictionary<Task>();

            return View(linguistRate);
        }

        // POST: BackOffice/Rates/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RateToLinguist")] LinguistRate linguistRate)
        {
            if (id != linguistRate.Id)
            {
                return NotFound();
            }

            //update the stored rate
            var storedLinguistRate = await _contextContainer.MainContext.LinguistRates.Include(cr => cr.Rate).Where(cr => cr.Id == id).AsNoTracking().FirstOrDefaultAsync();
            storedLinguistRate!.CustomRateToLinguist = linguistRate.CustomRateToLinguist;

            ModelState.Remove("Linguist");
            ModelState.Remove("Rate");
            if (ModelState.IsValid)
            {
                try
                {
                    _contextContainer.MainContext.Update(storedLinguistRate);
                    await _contextContainer.MainContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return RedirectToAction(nameof(Index), new { linguistId = storedLinguistRate.LinguistId });
            }
            return View(linguistRate);
        }

        // GET: BackOffice/Rates/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _contextContainer.MainContext.Rates == null)
            {
                return NotFound();
            }

            var rate = await _contextContainer.MainContext.Rates
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
            if (_contextContainer.MainContext.Rates == null)
            {
                return Problem("Entity set 'MainDbContext.Rates'  is null.");
            }
            var rate = await _contextContainer.MainContext.Rates.FindAsync(id);
            if (rate != null)
            {
                _contextContainer.MainContext.Rates.Remove(rate);
            }

            await _contextContainer.MainContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
