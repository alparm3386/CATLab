﻿using CAT.Models.Common;
using CAT.Models.Entities.Main;

namespace CAT.Services.Common
{
    public interface IQuoteService
    {
        Task<Quote> CreateQuoteFromTempQuoteAsync(int tempQuoteId);
        public Task<StoredQuote> CreateStoredQuoteAsync(int clientId);
        public Task<List<TempQuote>> CreateTempQuotesAsync(int storedQuoteId, int clientId, int sourceLanguage, int[] targetLanguages, 
            int speciality, int service, int idDocument, bool clientReview);
        Task DeleteStoredQuoteAsync(StoredQuote storedQuote);
        public Task<StoredQuote?> GetStoredQuoteAsync(int storedQuoteId);

        public Task<List<StoredQuote>> GetStoredQuotesAsync(DateTime from, DateTime to);
    }
}
