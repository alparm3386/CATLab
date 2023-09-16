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
    public class ClientFiltersController : Controller
    {
        private readonly MainDbContext _mainDbContext;

        public ClientFiltersController(MainDbContext mainDbContext)
        {
            _mainDbContext = mainDbContext;
        }

        // GET: BackOffice/Rates
        public async Task<IActionResult> Index(int? companyId)
        {
            try
            {
                //get the company
                var company = _mainDbContext.Companies.Where(c => c.Id == companyId).AsNoTracking().FirstOrDefault();
                if (company == null)
                    throw new CATException("Invalid company");
                ViewData["CompanyId"] = company.Id;
                ViewData["CompanyName"] = company.Name;

                //get the rates
                var filters = await _mainDbContext.Filters.AsNoTracking().Where(f => f.CompanyId == companyId).ToListAsync();
                return View(filters);
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

        // POST: BackOffice/Rates/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadFilter(int? companyId)
        {
            try
            {
                if (!companyId.HasValue)
                    throw new CATException("Invalid company");

                return RedirectToAction(nameof(Index), new { companyId });
            }
            catch (Exception ex)
            {
                // set error message here that is displayed in the view
                if (ex is CATException)
                    ViewData["ErrorMessage"] = ex.Message;
                else
                    ViewData["ErrorMessage"] = "There was an error processing your request. Please try again later.";
                // Optionally log the error: _logger.LogError(ex, "Error message here");

                return View();
            }
        }

        // POST: BackOffice/Rates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_mainDbContext.Rates == null)
            {
                return Problem("Entity set 'MainDbContext.Rates'  is null.");
            }
            var rate = await _mainDbContext.Rates.FindAsync(id);
            if (rate != null)
            {
                _mainDbContext.Rates.Remove(rate);
            }

            await _mainDbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
