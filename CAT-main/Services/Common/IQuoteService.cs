using CAT.Models.Common;
using CAT.Models.Entities.Main;

namespace CAT.Services.Common
{
    public interface IQuoteService
    {
        public Task<StoredQuote> CreateStoredQuoteAsync(int clientId);
        public Task<List<TempQuote>> CreateTempQuotesAsync(int storedQuoteId, int clientId, LocaleId sourceLocale, LocaleId[] targetLocales, 
            int speciality, int service, int idDocument);

        public Task<StoredQuote?> GetStoredQuoteAsync(int storedQuoteId);

        public Task<List<StoredQuote>> GetStoredQuotesAsync(DateTime from, DateTime to);

        public Task LaunchStoredQuotesAsync(int idStoredQuote);
    }
}
