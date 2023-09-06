using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CAT.Data;
using CAT.Models.Entities.Main;
using CAT.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using IdentityDbContext = CAT.Areas.Identity.Data.IdentityDbContext;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.Design;

namespace CAT.Areas.BackOffice.Controllers
{
    [Area("BackOffice")]
    public class ClientsController : Controller
    {
        private readonly MainDbContext _mainDbContext;
        private readonly IdentityDbContext _identityDbContext;
        private readonly ILogger _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClientsController(MainDbContext mainDbContext, IdentityDbContext identityDbContext, UserManager<ApplicationUser> userManager,
            ILogger<ClientsController> logger)
        {
            _mainDbContext = mainDbContext;
            _identityDbContext = identityDbContext;
            _logger = logger;
            _userManager = userManager;
        }

        // GET: BackOffice/Clients
        public async Task<IActionResult> Index(int companyId)
        {
            try
            {
                var clients = await _mainDbContext.Clients.Include(c => c.Address).Include(c => c.Company).Where(c => c.CompanyId == companyId).ToListAsync();

                return View(clients);
            }
            catch (Exception)
            {
                // set error message here that is displayed in the view
                ViewData["ErrorMessage"] = "There was an error processing your request. Please try again later.";
                // Optionally log the error: _logger.LogError(ex, "Error message here");
            }

            return View();
        }

        // GET: BackOffice/Clients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null || _mainDbContext.Clients == null)
                    throw new Exception("Client not found.");

                var client = await _mainDbContext.Clients
                    .Include(c => c.Address)
                    .Include(c => c.Company)
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (client == null)
                {
                    return NotFound();
                }

                return View(client);
            }
            catch (Exception ex)
            {
                // set error message here that is displayed in the view
                ViewData["ErrorMessage"] = ex.Message;
                // Optionally log the error: _logger.LogError(ex, "Error message here");
            }

            return View();
        }

        // GET: BackOffice/Clients/Create
        public async Task<IActionResult> Create(int companyId)
        {
            try
            {
                var company = await _mainDbContext.Companies.FirstOrDefaultAsync(c => c.Id == companyId);
                if (company == null)
                    throw new Exception("Invalid company");
                var client = new Client() { CompanyId = companyId, Company = company };

                return View(client);
            }
            catch (Exception)
            {
                // set error message here that is displayed in the view
                ViewData["ErrorMessage"] = "There was an error processing your request. Please try again later.";
                // Optionally log the error: _logger.LogError(ex, "Error message here");
            }

            return View();
        }

        // POST: BackOffice/Clients/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,User,Address,CompanyId,Company.Name")] Client client)
        {
            try
            {
                ModelState.Remove("Company");
                ModelState.Remove("UserId");
                ModelState.Remove("Company.Address");
                if (ModelState.IsValid)
                {
                    if (client.User.PasswordHash != client.User.SecurityStamp)
                        throw new Exception("passwords don't match.");
                    //save the user
                    var user = new ApplicationUser
                    {
                        UserName = client.User.UserName,
                        Email = client.User.Email
                    };

                    // Use UserManager to create a user
                    var result = await _userManager.CreateAsync(user, client.User.PasswordHash!);

                    if (result.Succeeded)
                        client.UserId = user.Id;
                    else
                    {
                        var errorMessages = result.Errors.Select(e => e.Description);
                        throw new Exception(string.Join(" ", errorMessages));
                    }
                    //save the address
                    _mainDbContext.Addresses.Add(client.Address);
                    await _mainDbContext.SaveChangesAsync();

                    //save the client
                    client.UserId = user.Id;
                    //client.AddressId = client.Address.Id;
                    client.Company = null;
                    _mainDbContext.Clients.Add(client);

                    await _mainDbContext.SaveChangesAsync();
                    return RedirectToAction(nameof(Index), new { companyId = client.CompanyId });
                }
                else
                {
                    var allErrors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                    ViewData["ErrorMessage"] = string.Join(" ", allErrors);
                    return View(client);
                }
            }
            catch (Exception ex)
            {
                // set error message here that is displayed in the view
                ViewData["ErrorMessage"] = ex.Message;
                // Optionally log the error: _logger.LogError(ex, "Error message here");
            }

            return View(client);
        }

        // GET: BackOffice/Clients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _mainDbContext.Clients == null)
            {
                return NotFound();
            }

            var client = await _mainDbContext.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: BackOffice/Clients/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,UserId,AddressId")] Client client)
        {
            if (id != client.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _mainDbContext.Update(client);
                    await _mainDbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(client.Id))
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

            return View(client);
        }

        // GET: BackOffice/Clients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _mainDbContext.Clients == null)
            {
                return NotFound();
            }

            var client = await _mainDbContext.Clients
                .Include(c => c.Address)
                .Include(c => c.Company)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: BackOffice/Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_mainDbContext.Clients == null)
            {
                return Problem("Entity set 'MainDbContext.Clients'  is null.");
            }
            var client = await _mainDbContext.Clients.FindAsync(id);
            if (client != null)
            {
                _mainDbContext.Clients.Remove(client);
            }

            await _mainDbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClientExists(int id)
        {
            return (_mainDbContext.Clients?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
