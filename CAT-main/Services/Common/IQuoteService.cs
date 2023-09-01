using CAT.Models.Common;
using CAT.Models.Entities.Main;

namespace CAT.Services.Common
{
    public interface IQuoteService
    {
        Task<Quote> CreateQuoteFromTempQuoteAsync(int tempQuoteId);
        public Task<StoredQuote> CreateStoredQuoteAsync(int clientId);
        public Task<List<TempQuote>> CreateTempQuotesAsync(int storedQuoteId, int clientId, LocaleId sourceLocale, LocaleId[] targetLocales, 
            int speciality, int service, int idDocument, bool clientReview);

        public Task<StoredQuote?> GetStoredQuoteAsync(int storedQuoteId);

        public Task<List<StoredQuote>> GetStoredQuotesAsync(DateTime from, DateTime to);
    }
}
