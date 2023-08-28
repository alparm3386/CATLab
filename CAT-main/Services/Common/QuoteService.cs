﻿using AutoMapper;
using CAT.Data;
using CAT.Models.Common;
using CAT.Models.Entities.Main;

namespace CAT.Services.Common
{
    public class QuoteService : IQuoteService
    {
        private readonly DbContextContainer _dbContextContainer;
        private readonly IConfiguration _configuration;
        private readonly CATConnector _catConnector;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public QuoteService(DbContextContainer dbContextContainer, IConfiguration configuration, CATConnector catConnector,
            IMapper mapper, ILogger<JobService> logger)
        {
            _dbContextContainer = dbContextContainer;
            _configuration = configuration;
            _catConnector = catConnector;
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

        public async Task<List<TempQuote>> CreateTempQuoteAsync(int storedQuote, int clientId, LocaleId sourceLocale, LocaleId[] targetLocales, int speciality, int idDocument, int idFilter)
        {
            try
            {
                //get the document
                var sourceDoc = _dbContextContainer.MainContext.Documents.FirstOrDefault(doc => doc.Id == idDocument);
                var sourceFilesFolder = Path.Combine(_configuration["SourceFilesFolder"]!);
                var filePath = Path.Combine(sourceFilesFolder, sourceDoc!.FileName!);

                //get the filter
                string? filterPath = null;
                var docfFilter = _dbContextContainer.MainContext.DocumentFilters.FirstOrDefault(docFilter => docFilter.DocumentId == idDocument);
                if (docfFilter != null)
                {
                    var filter = _dbContextContainer.MainContext.Filters.FirstOrDefault(filter => filter.Id == docfFilter.FilterId)!;
                    var fileFiltersFolder = Path.Combine(_configuration["FileFiltersFolder"]!);
                    filterPath = Path.Combine(fileFiltersFolder, filter.FilterName!);
                }

                //get the analisys
                var targetLanguages = Array.ConvertAll(targetLocales, locale => locale.Language);

                var tmAssignments = new List<Models.Common.TMAssignment>() { new Models.Common.TMAssignment() { tmId = "29610/__35462_en_fr" } };
                var stats =
                    _catConnector.GetStatisticsForDocument(filePath, filterPath!, sourceLocale.Language, targetLanguages, tmAssignments.ToArray());

                var quotes = new List<Quote>();
                foreach (var stat in stats)
                {
                    //save the analisys
                    var analisys = new Analysis()
                    {
                        DocumentId = idDocument,
                        SourceLanguage = sourceLocale.Language,
                        TargetLanguage = stat.targetLang,
                        Type = Enums.AnalysisType.Normal,
                        Repetitions = stat.repetitions,
                        Match_101 = stat.match_101,
                        Match_100 = stat.match_100,
                        Match_95_99 = stat.match_95_99,
                        Match_85_94 = stat.match_85_94,
                        Match_75_84 = stat.match_75_84,
                        Match_50_74 = stat.match_50_74,
                        No_match = stat.no_match,
                        Speciality = speciality
                    };

                    _dbContextContainer.MainContext.Analisys.Add(analisys);

                    //create and save the quote
                    var quote = new Quote()
                    {
                        SourceLanguage = sourceLocale.Language,
                        TargetLanguage = stat.targetLang!,
                        Speciality = speciality,
                        DateCreated = DateTime.Now,
                        Service = 1,
                        Fee = 10.0
                    };

                    quotes.Add(quote);
                }

                // Add the quotes
                await _dbContextContainer.MainContext.Quotes.AddRangeAsync(quotes);

                //save the changes
                await _dbContextContainer.MainContext.SaveChangesAsync();

                //return _mapper.Map<QuoteDto[]>(quotes);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError("CreateQuote ERROR:" + ex.Message);
                throw;
            }
        }
    }
}
