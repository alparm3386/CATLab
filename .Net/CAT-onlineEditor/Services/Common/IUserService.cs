using CAT.Areas.Identity.Data;

namespace CAT.Services.Common
{
    public interface IUserService
    {
        Task<ApplicationUser> GetCurrentUserAsync();
    }
}