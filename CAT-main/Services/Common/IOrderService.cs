using CAT.Models.Entities.Main;

namespace CAT.Services.Common
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(int clientId);
        public Task LaunchStoredQuotesAsync(int idStoredQuote);
    }
}