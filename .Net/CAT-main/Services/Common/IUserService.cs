using CAT.Areas.Identity.Data;
using CAT.Models.Entities.Main;

namespace CAT.Services.Common
{
    public interface IUserService
    {
        Task<Client> GetClient(int clientId);
        string GetCurrentUserId();
        Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName);
    }
}