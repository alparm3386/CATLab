using CATWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

public class AccountController : Controller
{
    // Other methods...

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken] // CSRF protection
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Assume this method checks the encrypted password in your database
            var result = await CheckLoginCredentials(model.Username, EncryptPassword(model.Password));
            if (result)
            {
                // Successfully logged in
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Invalid login attempt
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }
        }
        else
        {
            // Model state is not valid
            return View(model);
        }
    }

    // This is a simple encryption method, you might want to replace this with a more secure one.
    private string EncryptPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }

    // Assume this is the method to check the login credentials from your database
    private async Task<bool> CheckLoginCredentials(string username, string password)
    {
        // Check the username and password from your database
        // Return true if credentials are correct, otherwise return false

        return true; // Placeholder
    }
}
