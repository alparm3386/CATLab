using CAT.Models.Common;
using CAT.Models.Entities.Main;

namespace CAT.Services.Common
{
    public interface IQuoteService
    {
        public Task<StoredQuote> CreateStoredQuoteAsync(int clientId);
        public Task<List<TempQuote>> CreateTempQuoteAsync(int storedQuote, int clientId, LocaleId sourceLocale, LocaleId[] targetLocales, int speciality, int idDocument, int idFilter);
    }
}
