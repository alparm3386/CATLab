using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CAT.Data;
using CAT.Models.Entities.Main;
using Microsoft.AspNetCore.Identity;
using CAT.Areas.Identity.Data;
using CAT.Areas.BackOffice.Models.ViewModels;
using System.Net;
using System.Linq.Expressions;

namespace CAT.Areas.BackOffice.Controllers
{
    [Area("BackOffice")]
    public class LinguistsController : Controller
    {
        private readonly MainDbContext _mainDbContext;
        private readonly IdentityDbContext _identityDbContext;
        private readonly ILogger _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;

        public LinguistsController(MainDbContext mainDbContext, IdentityDbContext identityDbContext, UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore, ILogger<LinguistsController> logger)
        {
            _mainDbContext = mainDbContext;
            _identityDbContext = identityDbContext;
            _logger = logger;
            _userManager = userManager;
            _userStore = userStore;
        }

        // GET: BackOffice/Linguists
        public async Task<IActionResult> Index()
        {
            try
            {
                var linguists = await _mainDbContext.Linguists.ToListAsync();

                //join into the users table 
                linguists = (from linguist in linguists
                             join user in _identityDbContext.Users on linguist.UserId equals user.Id
                             select new Linguist
                             {
                                 Id = linguist.Id,
                                 UserId = linguist.UserId,
                                 LinguistsLanguagePairs = linguist.LinguistsLanguagePairs,
                                 User = user
                             }).ToList();

                return View(linguists);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "LinguistsController->Index");
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(new List<Linguist>());
            }
        }

        // GET: BackOffice/Linguists/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                var linguist = await _mainDbContext.Linguists.FirstOrDefaultAsync(m => m.Id == id);

                return View(linguist);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "LinguistsController->Index");
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(new List<Linguist>());
            }
        }

        // GET: BackOffice/Linguists/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BackOffice/Linguists/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Linguist linguist)
        {
            try
            {
                ModelState.Remove("UserId");
                ModelState.Remove("LinguistsLanguagePairs");
                if (ModelState.IsValid)
                {
                    if (linguist.User.PasswordHash != linguist.User.SecurityStamp)
                        throw new Exception("passwords don't match.");

                    //save the user
                    var user = new ApplicationUser
                    {
                        UserName = linguist.User.UserName,
                        Email = linguist.User.Email,
                        FirstName = linguist.User.FirstName,
                        LastName = linguist.User.LastName
                    };

                    await _userStore.SetUserNameAsync(user, user.UserName, CancellationToken.None);
                    var emailStore = (IUserEmailStore<ApplicationUser>)_userStore;
                    await emailStore.SetEmailAsync(user, user.Email, CancellationToken.None);

                    // Use UserManager to create a user
                    var result = await _userManager.CreateAsync(user, linguist.User.PasswordHash!);

                    if (result.Succeeded)
                    {
                        linguist.UserId = user.Id;
                        user.EmailConfirmed = true;
                        user.EmailConfirmed = true;
                        await _userManager.AddToRoleAsync(user, "Linguist");
                        await _userManager.UpdateAsync(user);
                    }
                    else
                    {
                        var errorMessages = result.Errors.Select(e => e.Description);
                        throw new Exception(string.Join(" ", errorMessages));
                    }

                    //save the address
                    _mainDbContext.Addresses.Add(linguist.Address);
                    await _mainDbContext.SaveChangesAsync();

                    //save the client
                    linguist.UserId = user.Id;

                    _mainDbContext.Linguists.Add(linguist);

                    await _mainDbContext.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "LinguistsController->Index");
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return View(linguist);
        }

        // GET: BackOffice/Linguists/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                //the client
                var linguist = await _mainDbContext.Linguists.Include(c => c.Address).FirstOrDefaultAsync(c => c.Id == id);

                //the user
                var user = await _identityDbContext.Users.Where(u => u.Id == linguist!.UserId).FirstOrDefaultAsync();
                linguist!.User = user!;

                return View(linguist);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "LinguistsController->Index");
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return View(new Client());
        }

        // POST: BackOffice/Linguists/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Linguist linguist)
        {
            try
            {
                //the client
                var storedLinguist = await _mainDbContext.Linguists.Include(c => c.Address).FirstOrDefaultAsync(c => c.Id == id);
                //the user
                var storedUser = await _identityDbContext.Users.Where(u => u.Id == linguist!.UserId).FirstOrDefaultAsync();

                ModelState.Remove("UserId");
                ModelState.Remove("LinguistsLanguagePairs");
                if (ModelState.IsValid)
                {
                    //set the address
                    storedLinguist!.Address.Line1 = linguist.Address.Line1;
                    storedLinguist.Address.Line2 = linguist.Address.Line2;
                    storedLinguist.Address.City = linguist.Address.City;
                    storedLinguist.Address.PostalCode = linguist.Address.PostalCode;
                    storedLinguist.Address.Country = linguist.Address.Country;
                    //storedLinguist.Address.Region = linguist.Address.Region;
                    storedLinguist.Address.Phone = linguist.Address.Phone;

                    //update the linguist
                    _mainDbContext.Update(storedLinguist);
                    await _mainDbContext.SaveChangesAsync();

                    //update the user
                    var user = await _userManager.FindByIdAsync(storedLinguist.UserId);
                    user!.Email = linguist.User.Email;
                    user.FirstName = linguist.User.FirstName;
                    user.LastName = linguist.User.LastName;
                    var result = await _userManager.UpdateAsync(user);
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }

                        return View(linguist);
                    }

                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "LinguistsController->Index");
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return View(linguist);
        }

        // GET: BackOffice/Linguists/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                throw new Exception("Operation is not allowed.");
                ////the client
                //var linguist = await _mainDbContext.Linguists.Include(c => c.Address).FirstOrDefaultAsync(c => c.Id == id);

                ////the user
                //var user = await _identityDbContext.Users.Where(u => u.Id == linguist!.UserId).FirstOrDefaultAsync();
                //linguist!.User = user!;

                //return View(linguist);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "LinguistsController->Index");
                ModelState.AddModelError(string.Empty, ex.Message);

                return View(new Linguist());
            }
        }

        // POST: BackOffice/Linguists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                throw new Exception("Operation is not allowed.");
                ////the linguist
                //var linguist = await _mainDbContext.Linguists.Include(c => c.Address).FirstOrDefaultAsync(c => c.Id == id);

                //await _mainDbContext.SaveChangesAsync();
                //return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "LinguistsController->Index");
                ModelState.AddModelError(string.Empty, ex.Message);

                return View(new Linguist());
            }
        }
    }
}
