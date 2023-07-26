using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using CATWeb.Services;

namespace CATWeb.Controllers.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly JwtService _jwtService;

        public AuthController(SignInManager<IdentityUser> signInManager, JwtService jwtService)
        {
            _signInManager = signInManager;
            _jwtService = jwtService;
        }

        // Other actions (like Register, Logout) can go here

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

            if (!result.Succeeded)
            {
                return Unauthorized(); // or however you want to handle failed login attempts
            }

            var user = await _signInManager.UserManager.FindByEmailAsync(model.Email);
            var token = _jwtService.GenerateJWT(user);

            return Ok(new { Token = token });
        }
    }
}
