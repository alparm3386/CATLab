using CAT.Models.Common;
using CAT.Models.Entities.Main;

namespace CAT.Services.Common
{
    public interface IQuoteService
    {
        public StoredQuote CreateStoredQuote(int clientId);
        public List<TempQuote> CreateTempQuote(int storedQuote, int clientId, LocaleId sourceLocale, LocaleId[] targetLocales, int speciality, int idDocument, int idFilter);
    }
}
