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
        private readonly IConfiguration _configuration;

        public ClientFiltersController(MainDbContext mainDbContext, IConfiguration configuration)
        {
            _mainDbContext = mainDbContext;
            _configuration = configuration;
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

            return View(new PaginatedList<Filter>(new List<Filter>(), 0, 0, 1));
        }

        // POST: BackOffice/Rates/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadFilter(int companyId, IFormFile FilterToUpload)
        {
            try
            {
                //check if the company exists
                var company = await _mainDbContext.Companies.FindAsync(companyId);
                if (company == null)
                    throw new CATException("Company not found.");

                //save the file
                var fileFiltersFolder = Path.Combine(_configuration["FileFiltersFolder"]!, companyId.ToString());
                var filterPath = Path.Combine(fileFiltersFolder, FilterToUpload!.FileName);
                if (System.IO.File.Exists(filterPath))
                    throw new CATException("The filter already exists.");

                // Ensure the directory exists
                if (!Directory.Exists(fileFiltersFolder))
                    Directory.CreateDirectory(fileFiltersFolder);

                using var fileStream = new FileStream(filterPath, FileMode.Create);
                await FilterToUpload.CopyToAsync(fileStream);

                //add the filter to the database
                _mainDbContext.Filters.Add(new Filter() { CompanyId = companyId, FilterName = FilterToUpload.FileName, FileTypes = "*" });
                _mainDbContext.SaveChanges();

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

        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                var filter = await _mainDbContext.Filters.FirstOrDefaultAsync(m => m.Id == id);
                if (filter == null)
                    throw new CATException("Filter not found.");

                return View(filter);
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
                return Problem("Entity set 'MainDbContext.Rates'  is null.");

            var filter = await _mainDbContext.Filters.FindAsync(id);
            if (filter != null)
                _mainDbContext.Filters.Remove(filter);

            await _mainDbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
