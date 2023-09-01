using AutoMapper;
using CAT.Data;
using CAT.Models.Entities.Main;

namespace CAT.Services.Common
{
    public class OrderService : IOrderService
    {
        private readonly DbContextContainer _dbContextContainer;
        private readonly IConfiguration _configuration;
        private readonly IQuoteService _quoteService;
        private readonly IDocumentService _documentService;
        private readonly IWorkflowService _workflowService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public OrderService(DbContextContainer dbContextContainer, IConfiguration configuration,
            IDocumentService documentService, IWorkflowService workflowService, IQuoteService quoteService, IMapper mapper, ILogger<JobService> logger) 
        {
            _dbContextContainer = dbContextContainer;
            _configuration = configuration;
            _quoteService = quoteService;
            _documentService = documentService;
            _workflowService = workflowService;
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

            //create quotes from temp quote
            foreach (var tempQuote in storedQuote!.TempQuotes)
            {
                //create document from temp document
                var document = await _documentService.CreateDocumentFromTempDocumentAsync(tempQuote.TempDocumentId);
                
                //create quote
                var quote = await _quoteService.CreateQuoteFromTempQuoteAsync(tempQuote.Id);

                //add job to the order
                var job = await AddJobToOrderAsync(order.Id, quote.Id, document.Id);

                //finalize the order
                await FinalizeOrderAsync(order.Id);

                //update the order id in the stored quote
                storedQuote.OrderId = order.Id;
                await _dbContextContainer.MainContext.SaveChangesAsync();
            }
        }

        public async Task<Job> AddJobToOrderAsync(int orderId, int quoteId, int documentId)
        {
            //create job
            var job = new Job()
            {
                OrderId = orderId,
                QuoteId = quoteId,
                SourceDocumentId = documentId
            };

            await _dbContextContainer.MainContext.Jobs.AddAsync(job);
            await _dbContextContainer.MainContext.SaveChangesAsync();

            return job;
        }

        public async Task FinalizeOrderAsync(int orderId)
        {
            await _workflowService.CreateWorkflowAsync(orderId);
        }
    }
}
