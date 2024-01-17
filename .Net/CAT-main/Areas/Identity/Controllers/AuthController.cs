using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using CAT.Services;
using CAT.Areas.Identity.Pages.Account;
using CAT.Areas.Identity.Data;

namespace CAT.Areas.Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtService _jwtService;

        public AuthController(SignInManager<ApplicationUser> signInManager, JwtService jwtService)
        {
            _signInManager = signInManager;
            _jwtService = jwtService;
        }

        // Other actions (like Register, Logout) can go here

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Input.Username, model.Input.Password, false, false);

            if (!result.Succeeded)
            {
                return Unauthorized(); // or however you want to handle failed login attempts
            }

            var user = await _signInManager.UserManager.FindByEmailAsync(model.Input.Username);
            var token = _jwtService.GenerateJWT(user!);

            return Ok(new { Token = token });
        }
    }
}
