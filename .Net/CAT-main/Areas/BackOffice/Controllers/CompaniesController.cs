using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CAT.Models.Entities.Main;
using CAT.Data;
using CAT.Areas.Identity.Data;
using CAT.Services.Common;

namespace CAT.Areas.BackOffice.Controllers
{
    [Area("BackOffice")]
    public class CompaniesController : Controller
    {
        private readonly MainDbContext _context;
        private readonly IdentityDbContext _identityDbContext;
        private readonly IUserService _userService;

        public CompaniesController(MainDbContext context, IdentityDbContext identityDbContext, IUserService userService)
        {
            _context = context;
            _identityDbContext = identityDbContext;
            _userService = userService;
        }

        // GET: BackOffice/Companies
        public async Task<IActionResult> Index()
        {
            try
            {
                return View(await _context.Companies.Include(c => c.Address).ToListAsync());
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "LinguistsController->Index");
                ViewData["ErrorMessage"] = ex.Message;
                return View(new List<Company>());
            }
        }

        // GET: BackOffice/Companies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                var company = await _context.Companies
                    .Include(c => c.Address)
                    .FirstOrDefaultAsync(c => c.Id == id);
                var pmUser = await _identityDbContext.Users.Where(u => u.Id == company!.PMId).FirstOrDefaultAsync();
                company!.ProjectManager = pmUser!;

                if (company == null)
                    throw new Exception("Not found.");

                return View(company);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "LinguistsController->Index");
                ViewData["ErrorMessage"] = ex.Message;
                return View();
            }
        }

        // GET: BackOffice/Companies/Create
        public async Task<IActionResult> Create()
        {
            var adminUsers = await _userService.GetUsersInRoleAsync("Admin");
            ViewData["PMs"] = new SelectList(adminUsers, "Id", "FullName");
            return View();
        }

        // POST: BackOffice/Companies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,PMId, Address")] Company company)
        {
            try
            {
                ModelState.Remove("ProjectManager");
                if (!ModelState.IsValid)
                    throw new Exception("Invalid model state.");

                _context.Add(company);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "LinguistsController->Index");
                ViewData["ErrorMessage"] = ex.Message;
                return View(company);
            }
        }

        // GET: BackOffice/Companies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                var company = await _context.Companies
                    .Include(c => c.Address)
                    .FirstOrDefaultAsync(c => c.Id == id);
                var pmUser = await _identityDbContext.Users.Where(u => u.Id == company!.PMId).FirstOrDefaultAsync();
                company!.ProjectManager = pmUser!;

                var adminUsers = await _userService.GetUsersInRoleAsync("Admin");
                ViewData["PMs"] = new SelectList(adminUsers, "Id", "FullName");

                if (company == null)
                    throw new Exception("Not found.");

                return View(company);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "LinguistsController->Index");
                ViewData["ErrorMessage"] = ex.Message;
                return View();
            }
        }

        // POST: BackOffice/Companies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,PMId,Address")] Company company)
        {
            try
            {
                var storedCompany = await _context.Companies
                    .Include(c => c.Address)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (storedCompany == null)
                    throw new Exception("Company not found.");

                ModelState.Remove("ProjectManager");
                if (!ModelState.IsValid)
                    throw new Exception("Invalid model state.");

                //the comany
                storedCompany.Name = company.Name;

                //the address
                storedCompany!.Address.Line1 = company.Address.Line1;
                storedCompany.Address.Line2 = company.Address.Line2;
                storedCompany.Address.City = company.Address.City;
                storedCompany.Address.PostalCode = company.Address.PostalCode;
                storedCompany.Address.Country = company.Address.Country;
                //storedLinguist.Address.Region = linguist.Address.Region;
                storedCompany.Address.Phone = company.Address.Phone;
                storedCompany.PMId = company.PMId;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "LinguistsController->Index");
                ViewData["ErrorMessage"] = ex.Message;
                return View();
            }
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
    }
}
