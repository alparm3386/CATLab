using AutoMapper;
using CAT.Data;
using CAT.Enums;
using CAT.Helpers;
using CAT.Models.Common;
using CAT.Models.Entities.Main;
using CAT.Models.ViewModels;
using CAT.Services.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.Xml;
using System.Transactions;

namespace CAT.Areas.BackOffice.Controllers
{
    [Area("BackOffice")]
    public class QuotesController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IQuoteService _quoteService;
        private readonly IDocumentService _documentService;
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public QuotesController(IConfiguration configuration, IQuoteService quoteService,
            IDocumentService documentService, IOrderService orderService, IMapper mapper, ILogger<JobService> logger)
        {
            _configuration = configuration;
            _quoteService = quoteService;
            _documentService = documentService;
            _orderService = orderService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var storedQuotes = await _quoteService.GetStoredQuotesAsync(DateTime.MinValue, DateTime.MaxValue);
            var storedQuotesViewModel = new StoredQuotesViewModel()
            {
                StoredQuotes = storedQuotes
            };

            return View(storedQuotesViewModel);
            //return RedirectToAction("QuoteDetails"); // or wherever you want to redirect
        }

        //[HttpPost]
        //public IActionResult Index(QuotesViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        // Process the uploaded file and other data as needed
        //        // Save to database, call another service, etc.
        //        return RedirectToAction("Success"); // or wherever you wish to redirect after processing
        //    }

        //    return View(model);
        //}

        [HttpPost]
        public async Task<IActionResult> HandleQuote(CreateQuoteViewModel model, string action)
        {

            if (!ModelState.IsValid)
            {
                return View("Create", model);
            }

            switch (action)
            {
                case "CalculateQuote":
                    var storedQuoteId = model.StoredQuoteId;
                    try
                    {
                        //using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)) //MSDTC
                        //using (var transaction = context.Database.BeginTransaction())
                        {
                            //create stored quote if doesn't exists
                            var clientId = -1;
                            if (model.StoredQuoteId <= 0)
                            {
                                var stroedQuote = await _quoteService.CreateStoredQuoteAsync(clientId);
                                storedQuoteId = stroedQuote.Id;
                            }

                            int idFilter = -1;
                            var document = await _documentService.CreateTempDocumentAsync(model.FileToUpload!, DocumentType.Original, idFilter);
                            //create the quote
                            var targetLocales = model.TargetLanguages!.Select(lang => new LocaleId(lang)).ToArray();
                            var quotes = await _quoteService.CreateTempQuotesAsync(storedQuoteId, 1, new LocaleId(model.SourceLanguage!), targetLocales,
                                model.Speciality, model.Service, document.Id, model.ClientReview);

                            //scope.Complete();
                        }

                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, "An error occurred while calculating the quote. Please try again.");
                        return View("Create", model);
                    }

                    return RedirectToAction("StoredQuoteDetails", new { id = storedQuoteId });
                default:
                    return View("Create", model);
            }
        }

        [Route("Quotes/Create/{id?}")]
        public async Task<IActionResult> Create(int? id)
        {
            var storedQuoteId = id ?? -1;
            var createQuoteViewModel = new CreateQuoteViewModel();
            createQuoteViewModel.StoredQuoteId = storedQuoteId;

            return View(createQuoteViewModel);
        }

        //[HttpGet()]
        //[HttpGet("{idStoredQuote?}")]
        [Route("Quotes/StoredQuoteDetails/{id?}")]
        public async Task<IActionResult> StoredQuoteDetails(int? id)
        {
            var storedQuoteId = id ?? -1;
            var storedQuote = await _quoteService.GetStoredQuoteAsync(storedQuoteId);

            var storedQuoteViewModel = new StoredQuoteDetailsViewModel();

            storedQuoteViewModel.StoredQuote = storedQuote!;
            return View(storedQuoteViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LaunchStoredQuote(int id)
        {
            await _orderService.LaunchStoredQuotesAsync(id);

            return RedirectToAction("StoredQuoteDetails", new { id });
        }
    }
}
