using AutoMapper;
using CAT.Data;
using CAT.Models.Common;
using CAT.Models.Entities.Main;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CAT.Services.Common
{
    public class QuoteService : IQuoteService
    {
        private readonly DbContextContainer _dbContextContainer;
        private readonly IConfiguration _configuration;
        private readonly CATConnector _catConnector;
        private readonly IDocumentService _documentService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public QuoteService(DbContextContainer dbContextContainer, IConfiguration configuration, CATConnector catConnector, 
            IDocumentService documentService, IMapper mapper, ILogger<JobService> logger)
        {
            _dbContextContainer = dbContextContainer;
            _configuration = configuration;
            _catConnector = catConnector;
            _documentService = documentService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<StoredQuote> CreateStoredQuoteAsync(int clientId)
        {
            var storedQuote = new StoredQuote()
            {
                ClientId = clientId,
                DateCreated = DateTime.Now,
                OrderId = -1
            };

            _dbContextContainer.MainContext.StoredQuotes.Add(storedQuote);
            await _dbContextContainer.MainContext.SaveChangesAsync();
            return storedQuote;
        }

        public async Task<List<TempQuote>> CreateTempQuotesAsync(int storedQuoteId, int clientId, LocaleId sourceLocale,
            LocaleId[] targetLocales, int speciality, int service, int tempDocumentId, bool clientReview)
        {
            try
            {
                //get the document
                var sourceDoc = _dbContextContainer.MainContext.TempDocuments.FirstOrDefault(doc => doc.Id == tempDocumentId);
                var sourceFilesFolder = Path.Combine(_configuration["TempFolder"]!);
                var filePath = Path.Combine(sourceFilesFolder, sourceDoc!.FileName!);

                //get the filter
                string? filterPath = null;
                var docfFilter = _dbContextContainer.MainContext.DocumentFilters.FirstOrDefault(docFilter => docFilter.DocumentId == tempDocumentId);
                if (docfFilter != null)
                {
                    var filter = _dbContextContainer.MainContext.Filters.FirstOrDefault(filter => filter.Id == docfFilter.FilterId)!;
                    var fileFiltersFolder = Path.Combine(_configuration["FileFiltersFolder"]!);
                    filterPath = Path.Combine(fileFiltersFolder, filter.FilterName!);
                }

                //get the analisys
                var targetLanguages = Array.ConvertAll(targetLocales, locale => locale.Language);

                var tmAssignments = new List<TMAssignment>() { new TMAssignment() { tmId = "29610/__35462_en_fr" } };
                var stats =
                    _catConnector.GetStatisticsForDocument(filePath, filterPath!, sourceLocale.Language, targetLanguages, tmAssignments.ToArray());

                var tempQuotes = new List<TempQuote>();
                foreach (var stat in stats)
                {
                    //create and save the quote
                    var tempQuote = new TempQuote()
                    {
                        StoredQuoteId = storedQuoteId,
                        SourceLanguage = sourceLocale.Language,
                        TargetLanguage = stat.targetLang!,
                        SpecialityId = speciality,
                        DateCreated = DateTime.Now,
                        Service = service,
                        Fee = 10.0,
                        TempDocumentId = tempDocumentId,
                        Analysis = JsonConvert.SerializeObject(stat),
                        ClientReview = clientReview
                    };

                    tempQuotes.Add(tempQuote);
                }

                // Add the quotes
                await _dbContextContainer.MainContext.TempQuotes.AddRangeAsync(tempQuotes);

                //save the changes
                await _dbContextContainer.MainContext.SaveChangesAsync();

                return tempQuotes;
            }
            catch (Exception ex)
            {
                _logger.LogError("CreateQuote ERROR:" + ex.Message);
                throw;
            }
        }

        public async Task<StoredQuote?> GetStoredQuoteAsync(int storedQuoteId)
        {
            return await _dbContextContainer!.MainContext!.StoredQuotes!.Include(sq => sq.TempQuotes).ThenInclude(tq => tq.TempDocument).FirstOrDefaultAsync(quote => quote.Id == storedQuoteId);
        }

        public async Task<List<StoredQuote>> GetStoredQuotesAsync(DateTime from, DateTime to)
        {
            return await _dbContextContainer!.MainContext!.StoredQuotes!
                .Include(sq => sq.Client)
                .ThenInclude(c => c.Company)
                .Include(sq => sq.TempQuotes)
                .ThenInclude(tq => tq.TempDocument)
                .Where(quote => quote.DateCreated >= from && quote.DateCreated <= to).ToListAsync();
        }

        public async Task<Quote> CreateQuoteFromTempQuoteAsync(int tempQuoteId)
        {
            //get the temp quote
            var tempQuote = await _dbContextContainer.MainContext.TempQuotes.FirstOrDefaultAsync(q => q.Id == tempQuoteId);
            var stat = JsonConvert.DeserializeObject<Statistics>(tempQuote!.Analysis!);

            //create the quote
            var quote = new Quote()
            {
                DateCreated = DateTime.Now,
                Fee = tempQuote.Fee,
                Service = tempQuote.Service,
                SourceLanguage = tempQuote.SourceLanguage,
                TargetLanguage = tempQuote.TargetLanguage,
                Speciality = tempQuote.SpecialityId,
                Speed = tempQuote.Speed,
                ClientReview = tempQuote.ClientReview,
                Words = stat!.WordCount
            };

            //save the quote
            await _dbContextContainer.MainContext.Quotes.AddAsync(quote);
            await _dbContextContainer.MainContext.SaveChangesAsync();

            return quote;
        }
    }
}
