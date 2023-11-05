using CAT.Enums;
using CAT.Models.Common;
using CAT.Models.Entities.Main;

namespace CAT.Services.Common
{
    public interface IQuoteService
    {
        System.Threading.Tasks.Task<Quote> CreateQuoteFromTempQuoteAsync(int tempQuoteId);
        public Task<StoredQuote> CreateStoredQuoteAsync(int clientId);
        public Task<List<TempQuote>> CreateTempQuotesAsync(int storedQuoteId, int clientId, int sourceLanguage, int[] targetLanguages, 
            Speciality speciality, Service service, ServiceSpeed speed, int idDocument, bool clientReview);
        System.Threading.Tasks.Task DeleteStoredQuoteAsync(StoredQuote storedQuote);
        public Task<StoredQuote?> GetStoredQuoteAsync(int storedQuoteId, bool withClientDetails);

        public IQueryable<StoredQuote> GetStoredQuotes(DateTime from, DateTime to);
        System.Threading.Tasks.Task<TempQuote?> GetTempQuoteAsync(int tempQuoteId);
    }
}
