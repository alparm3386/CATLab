using CAT.Models.Common;
using CAT.Models.Entities.Main;

namespace CAT.Services.Common
{
    public interface IQuoteService
    {
        public List<TempQuote> CreateTempQuote(int storedQuote, int clientId, LocaleId sourceLocale, LocaleId[] targetLocales, int speciality, int idDocument, int idFilter);
    }
}
