using CAT.Models.Entities.Main;

namespace CAT.Services.Common
{
    public interface IOrderService
    {
        Task<Job> AddJobToOrderAsync(int orderId, int quoteId, int documentId);
        Task<Order> CreateOrderAsync(int clientId);
        Task FinalizeOrderAsync(int orderId);
        public Task LaunchStoredQuotesAsync(int idStoredQuote);
    }
}