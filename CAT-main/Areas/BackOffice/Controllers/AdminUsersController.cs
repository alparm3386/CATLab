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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using IdentityDbContext = CAT.Areas.Identity.Data.IdentityDbContext;

namespace CAT.Areas.BackOffice.Controllers
{
    [Area("BackOffice")]
    public class AdminUsersController : Controller
    {
        private readonly IdentityDbContext _identityDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserStore<ApplicationUser> _userStore;

        public AdminUsersController(IdentityDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
            IUserStore<ApplicationUser> userStore)
        {
            _identityDbContext = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _userStore = userStore;

            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
        }

        // GET: BackOffice/AdminUsers
        public async Task<IActionResult> Index()
        {
            var role = await _roleManager.FindByNameAsync("Admin");

            var adminUsers = await _userManager.GetUsersInRoleAsync(role!.Name!);
            
            return View(adminUsers);
        }

        // GET: BackOffice/AdminUsers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BackOffice/AdminUsers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,Email,PasswordHash,SecurityStamp")] ApplicationUser user)
        {
            try
            {
                //ModelState.Remove("Company");
                if (ModelState.IsValid)
                {
                    if (user.PasswordHash != user.SecurityStamp)
                        throw new Exception("passwords don't match.");
                    //save the user
                    var newUser = new ApplicationUser
                    {
                        UserName = user.UserName,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName
                    };

                    await _userStore.SetUserNameAsync(user, user.Email, CancellationToken.None);
                    var emailStore = (IUserEmailStore<ApplicationUser>)_userStore;
                    await emailStore.SetEmailAsync(user, user.Email, CancellationToken.None);

                    // Use UserManager to create a user
                    var result = await _userManager.CreateAsync(user, user.PasswordHash!);

                    if (result.Succeeded)
                    {
                        user.EmailConfirmed = true;
                        await _userManager.UpdateAsync(user);
                    }
                    else
                    {
                        var errorMessages = result.Errors.Select(e => e.Description);
                        throw new Exception(string.Join(" ", errorMessages));
                    }
                }
                else
                {
                    var allErrors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                    ViewData["ErrorMessage"] = string.Join(" ", allErrors);
                    return View(user);
                }
            }
            catch (Exception ex)
            {
                // set error message here that is displayed in the view
                ViewData["ErrorMessage"] = ex.Message;
                // Optionally log the error: _logger.LogError(ex, "Error message here");
                return View(user);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: BackOffice/AdminUsers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            //if (id == null || _identityDbContext.Clients == null)
            //{
            //    return NotFound();
            //}

            //var client = await _identityDbContext.Clients.FindAsync(id);
            //if (client == null)
            //{
            //    return NotFound();
            //}
            //ViewData["AddressId"] = new SelectList(_identityDbContext.Addresses, "Id", "City", client.AddressId);
            //ViewData["CompanyId"] = new SelectList(_identityDbContext.Companies, "Id", "Id", client.CompanyId);
            //return View(client);

            return View();
        }

        // POST: BackOffice/AdminUsers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,UserId,AddressId")] Client client)
        {
            //if (id != client.Id)
            //{
            //    return NotFound();
            //}

            //if (ModelState.IsValid)
            //{
            //    try
            //    {
            //        _identityDbContext.Update(client);
            //        await _identityDbContext.SaveChangesAsync();
            //    }
            //    catch (DbUpdateConcurrencyException)
            //    {
            //        if (!ClientExists(client.Id))
            //        {
            //            return NotFound();
            //        }
            //        else
            //        {
            //            throw;
            //        }
            //    }
            //    return RedirectToAction(nameof(Index));
            //}
            //ViewData["AddressId"] = new SelectList(_identityDbContext.Addresses, "Id", "City", client.AddressId);
            //ViewData["CompanyId"] = new SelectList(_identityDbContext.Companies, "Id", "Id", client.CompanyId);
            //return View(client);

            return View();
        }

        // GET: BackOffice/AdminUsers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            //if (id == null || _identityDbContext.Clients == null)
            //{
            //    return NotFound();
            //}

            //var client = await _identityDbContext.Clients
            //    .Include(c => c.Address)
            //    .Include(c => c.Company)
            //    .FirstOrDefaultAsync(m => m.Id == id);
            //if (client == null)
            //{
            //    return NotFound();
            //}

            //return View(client);

            return View();
        }

        // POST: BackOffice/AdminUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //if (_identityDbContext.Clients == null)
            //{
            //    return Problem("Entity set 'MainDbContext.Clients'  is null.");
            //}
            //var client = await _identityDbContext.Clients.FindAsync(id);
            //if (client != null)
            //{
            //    _identityDbContext.Clients.Remove(client);
            //}

            //await _identityDbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClientExists(int id)
        {
            return false;
//            return (_identityDbContext.Clients?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
