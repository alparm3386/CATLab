using AutoMapper;
using CAT.Data;
using CAT.Enums;
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
        private readonly ICATConnector _catConnector;
        private readonly IDocumentService _documentService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly ILanguageService _languageService;


        public QuoteService(DbContextContainer dbContextContainer, IConfiguration configuration, ICATConnector catConnector, 
            IDocumentService documentService, ILanguageService languageService, IMapper mapper, ILogger<JobService> logger)
        {
            _dbContextContainer = dbContextContainer;
            _configuration = configuration;
            _catConnector = catConnector;
            _documentService = documentService;
            _logger = logger;
            _mapper = mapper;
            _languageService = languageService;
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

        public async Task<List<TempQuote>> CreateTempQuotesAsync(int storedQuoteId, int clientId, int sourceLang,
            int[] targetLangs, Speciality speciality, Service service, ServiceSpeed speed, int tempDocumentId, bool clientReview)
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
                var client = _dbContextContainer.MainContext.Clients.FirstOrDefault(client => client.Id == clientId)!;
                var tmAssignments = _catConnector.GetTMAssignments(client.CompanyId, sourceLang, targetLangs, (int)speciality, false);
                var stats = await _catConnector.GetStatisticsForDocument(filePath, filterPath!, sourceLang,
                    targetLangs, tmAssignments.ToArray());

                var tempQuotes = new List<TempQuote>();
                foreach (var stat in stats)
                {
                    //create and save the quote
                    var tempQuote = new TempQuote()
                    {
                        StoredQuoteId = storedQuoteId,
                        SourceLanguage = sourceLang,
                        TargetLanguage = stat.targetLang,
                        SpecialityId = (int)speciality,
                        DateCreated = DateTime.Now,
                        Service = (int)service,
                        Speed = (int)speed,
                        Fee = CalculateClientFee(clientId, stats[0], service, speed),
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

        public async Task<StoredQuote?> GetStoredQuoteAsync(int storedQuoteId, bool withClientDetails)
        {
            var storedQuote = await _dbContextContainer!.MainContext!.StoredQuotes!
                .Include(sq => sq.Client)
                .Include(sq => sq.TempQuotes)
                .ThenInclude(tq => tq.TempDocument)
                .FirstOrDefaultAsync(quote => quote.Id == storedQuoteId);

            if (withClientDetails)
            {
                var user = await _dbContextContainer.IdentityContext.Users.Where(u => u.Id == storedQuote!.Client.UserId).FirstOrDefaultAsync();
                storedQuote!.Client.User = user!;
            }

            return storedQuote;
        }

        public async Task<TempQuote?> GetTempQuoteAsync(int tempQuoteId)
        {
            var tempQuote = await _dbContextContainer!.MainContext!.TempQuotes!
                .Include(quote => quote.TempDocument)
                .FirstOrDefaultAsync(quote => quote.Id == tempQuoteId);

 
            return tempQuote;
        }

        public async System.Threading.Tasks.Task DeleteStoredQuoteAsync(StoredQuote storedQuote)
        {
            _dbContextContainer!.MainContext!.StoredQuotes!.Remove(storedQuote);
            await _dbContextContainer.MainContext.SaveChangesAsync();
        }

        public IQueryable<StoredQuote> GetStoredQuotes(DateTime from, DateTime to)
        {
            return _dbContextContainer!.MainContext!.StoredQuotes!
                .Include(sq => sq.Client)
                .ThenInclude(c => c.Company)
                .Include(sq => sq.TempQuotes)
                .ThenInclude(tq => tq.TempDocument)
                .Where(quote => quote.DateCreated >= from && quote.DateCreated <= to)
                .OrderByDescending(sq => sq.DateCreated);  // Order by DateCreated descending;
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

        private double CalculateClientFee(int clientId, Statistics stats, Service service, ServiceSpeed speed)
        {
            //double clientFee = 0.0;
            var translationFee = stats.WordCount * 75 / 1000;

            return translationFee;
            //if (service == Service.AI)
            //{
            //    //rateToClient stats.WordCount
            //}
            //else if (service == Service.AIWithRevision)
            //{
            //}
            //else if (service == Service.AIWithTranslationAndRevision)
            //{
            //    //AI process

            //    //translation
            //    //revision

            //    //client review
            //}
            //else if (service == Service.TranslationWithRevision)
            //{
            //    //translation

            //    //revision

            //    //client review
            //}
            //else
            //    throw new NotImplementedException();

            //return clientFee;
        }
    }
}
