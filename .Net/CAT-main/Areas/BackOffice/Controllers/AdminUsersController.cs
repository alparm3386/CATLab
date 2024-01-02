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
using System.Data;

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
            try
            {
                var role = await _roleManager.FindByNameAsync("Admin");

                var adminUsers = await _userManager.GetUsersInRoleAsync(role!.Name!);

                return View(adminUsers);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = ex.Message;
                return View(new List<Linguist>());
            }
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
        public async Task<IActionResult> Create([Bind("FirstName,LastName,UserName,Email,PasswordHash,SecurityStamp")] ApplicationUser user)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (user.PasswordHash != user.SecurityStamp)
                        throw new Exception("passwords don't match.");

                    //save the user
                    await _userStore.SetUserNameAsync(user, user.UserName, CancellationToken.None);
                    var emailStore = (IUserEmailStore<ApplicationUser>)_userStore;
                    await emailStore.SetEmailAsync(user, user.Email, CancellationToken.None);

                    // Use UserManager to create a user
                    var result = await _userManager.CreateAsync(user, user.PasswordHash!);

                    if (result.Succeeded)
                    {
                        user.EmailConfirmed = true;
                        await _userManager.AddToRoleAsync(user, "Admin");
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
                return View(user);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: BackOffice/AdminUsers/Edit/5
        public async Task<IActionResult> Edit(string? id)
        {
            try
            {
                var adminUser = await _userManager.FindByIdAsync(id!);

                return View(adminUser);
            }
            catch (Exception ex)
            {
                // set error message here that is displayed in the view
                ViewData["ErrorMessage"] = ex.Message;
                return View(new ApplicationUser());
            }
        }

        // POST: BackOffice/AdminUsers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("FirstName,LastName,Email")] ApplicationUser user)
        {
            try
            {
                var adminUser = await _userManager.FindByIdAsync(id!);
                adminUser!.FirstName = user.FirstName;
                adminUser.LastName = user.LastName;
                adminUser.Email = user.Email;

                var result = await _userManager.UpdateAsync(adminUser);
                if (result.Succeeded)
                {
                    // User was updated successfully
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    // Handle the errors and possibly return a view or another action
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // set error message here that is displayed in the view
                ViewData["ErrorMessage"] = ex.Message;

                user.Id = id;
                return View(user);
            }
        }

        // GET: BackOffice/AdminUsers/Delete/5
        public async Task<IActionResult> Delete(string? id)
        {
            ViewData["ErrorMessage"] = "Operation not permitted.";

            try
            {
                var adminUser = await _userManager.FindByIdAsync(id!);

                return View(adminUser);
            }
            catch (Exception ex)
            {
                // set error message here that is displayed in the view
                ViewData["ErrorMessage"] = ex.Message;
                return View(new ApplicationUser());
            }
        }

        // POST: BackOffice/AdminUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ViewData["ErrorMessage"] = "Operation not permitted.";
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
            return View(new ApplicationUser());
        }
    }
}
