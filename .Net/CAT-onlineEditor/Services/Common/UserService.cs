using CAT.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace CAT.Services.Common
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task<ApplicationUser> GetCurrentUserAsync()
        {
            // Get the current user's identity from HttpContext
            var userIdentity = _httpContextAccessor.HttpContext?.User;

            // Check if the user is authenticated
            if (userIdentity?.Identity?.IsAuthenticated ?? false)
            {
                // Retrieve user-related data (e.g., user ID, username, roles) as needed
                var userName = userIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Retrieve user roles
                var currentUser = await _userManager.FindByNameAsync(userName!);
                var roles = await _userManager.GetRolesAsync(currentUser!);

                if (roles.Contains("Admin"))
                    currentUser!.UserType = Enums.UserType.Admin;
                else if (roles.Contains("Client"))
                    currentUser!.UserType = Enums.UserType.Client;
                else if (roles.Contains("Linguist"))
                    currentUser!.UserType = Enums.UserType.Linguist;
                else
                    currentUser!.UserType = Enums.UserType.Unknown;

                return currentUser;
            }

            // Return null if the user is not authenticated
            return null!;
        }
    }
}
