using CAT.Models.Common;
using CAT.Models.DTOs;

namespace CAT.Services.Common
{
    public interface IQuoteService
    {
        public QuoteDto[] CreateQuote(int clientId, LocaleId sourceLocale, LocaleId[] targetLocales, int speciality, int idDocument, int idFilter);
    }
}
