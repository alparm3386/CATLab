using CAT.Models.Entities.Main;

namespace CAT.Services.Common
{
    public interface IOrderService
    {
        Task<Job> AddJobToOrderAsync(int orderId, int quoteId, int documentId);
        Task<Order> CreateOrderAsync(int clientId);
        public Task LaunchStoredQuotesAsync(int idStoredQuote);
    }
}