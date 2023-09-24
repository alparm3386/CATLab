using CAT.Models.Entities.Main;

namespace CAT.Services.Common
{
    public interface IUserService
    {
        Task<Client> GetClient(int clientId);
    }
}