using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using CAT.Services;
using CAT.Areas.Identity.Data;
using NuGet.Common;

namespace CAT.Controllers.Api
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
            try
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
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }

            //// create the cookie
            //var cookieOptions = new CookieOptions
            //{
            //    HttpOnly = true, // make the cookie inaccessible to javascript
            //    Expires = DateTime.UtcNow.AddHours(1), // set cookie expiration date
            //    Secure = true, // transmit the cookie only over HTTPS
            //    SameSite = SameSiteMode.Strict, // prevents the browser from sending the cookie along with cross-site requests
            //};

            //// set the cookie
            //Response.Cookies.Append("jwt", token, cookieOptions);

            //// redirect to home or return a success message
            //return Ok(new { Message = "Successfully logged in" });
        }
    }
}
