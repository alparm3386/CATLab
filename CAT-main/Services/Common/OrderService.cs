using AutoMapper;
using CAT.Data;
using CAT.Models.Entities.Main;

namespace CAT.Services.Common
{
    public class OrderService : IOrderService
    {
        private readonly DbContextContainer _dbContextContainer;
        private readonly IConfiguration _configuration;
        private readonly CATConnector _catConnector;
        private readonly IQuoteService _quoteService;
        private readonly IDocumentService _documentService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public OrderService(DbContextContainer dbContextContainer, IConfiguration configuration, CATConnector catConnector,
            IDocumentService documentService, IQuoteService quoteService, IMapper mapper, ILogger<JobService> logger) {
            _dbContextContainer = dbContextContainer;
            _configuration = configuration;
            _catConnector = catConnector;
            _quoteService = quoteService;
            _documentService = documentService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Order> CreateOrderAsync(int clientId)
        {
            //create an order
            var order = new Order()
            {
                ClientId = clientId,
                DateCreated = DateTime.Now
            };

            await _dbContextContainer.MainContext.Orders.AddAsync(order);
            await _dbContextContainer.MainContext.SaveChangesAsync();

            return order;
        }

        public async Task LaunchStoredQuotesAsync(int idStoredQuote)
        {
            var storedQuote = await _quoteService.GetStoredQuoteAsync(idStoredQuote);

            //create an order
            var order = await CreateOrderAsync(storedQuote!.ClientId);

            await _dbContextContainer.MainContext.Orders.AddAsync(order);

            //create quotes from temp quote
            foreach (var tempQuote in storedQuote!.TempQuotes)
            {
                //create document from temp document
                var document = await _documentService.CreateDocumentFromTempDocumentAsync(tempQuote.TempDocumentId);
                await _dbContextContainer.MainContext.Documents.AddAsync(document);
            }
        }

    }
}
