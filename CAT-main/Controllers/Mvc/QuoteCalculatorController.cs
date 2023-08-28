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
using System.Transactions;

namespace CAT.Controllers.Mvc
{
    public class QuoteCalculatorController : Controller
    {
        private readonly DbContextContainer _dbContextContainer;
        private readonly IConfiguration _configuration;
        private readonly IQuoteService _quoteService;
        private readonly IDocumentService _documentService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public QuoteCalculatorController(DbContextContainer dbContextContainer, IConfiguration configuration, IQuoteService quoteService,
            IDocumentService documentService, IMapper mapper, ILogger<JobService> logger)
        {
            _dbContextContainer = dbContextContainer;
            _configuration = configuration;
            _quoteService = quoteService;
            _documentService = documentService;
            _logger = logger;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            return RedirectToAction("QuoteDetails"); // or wherever you want to redirect
            //return View(new QuoteCalculatorViewModel());
        }

        //[HttpPost]
        //public IActionResult Index(QuoteCalculatorViewModel model)
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
        public async Task<IActionResult> HandleQuote(QuoteCalculatorViewModel model, string action)
        {

            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            switch (action)
            {
                case "CalculateQuote":
                    try
                    {
                        //using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)) //MSDTC
                        //using (var transaction = context.Database.BeginTransaction())
                        {
                            var document = await _documentService.CreateDocumentAsync(model.FileToUpload!, DocumentType.Original);
                            //create the quote
                            var targetLocales = model.TargetLanguages!.Select(lang => new LocaleId(lang)).ToArray();
                            var quotes = _quoteService.CreateTempQuote(1, 1, new LocaleId(model.SourceLanguage!), targetLocales,
                                model.Speciality, document.Id, model.Filter);

                            //scope.Complete();
                        }

                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, "An error occurred while calculating the quote. Please try again.");
                        return View("Index", model);
                    }
                    return RedirectToAction("QuoteDetails"); // or wherever you want to redirect
                default:
                    return View("Index", model);
            }
        }

        //[HttpGet()]
        //[HttpGet("{idStoredQuote?}")]
        [Route("QuoteCalculator/QuoteDetails/{idStoredQuote?}")]
        public async Task<IActionResult> QuoteDetails(int? idStoredQuote)
        {
            var quote = await _dbContextContainer.MainContext.Quotes.FirstOrDefaultAsync(quote => quote.Id == idStoredQuote);
            if (quote == null)
            {
                return NotFound();
            }

            return View(quote);
        }

        [HttpPost]
        public async Task<IActionResult> LaunchQuote(int id)
        {
            var quote = await _dbContextContainer.MainContext.Quotes.FindAsync(id);
            if (quote == null)
            {
                return NotFound();
            }

            // Your business logic to launch the quote.
            // ...

            ViewBag.Message = "Quote launched successfully!";
            return View("QuoteDetails", quote);
        }
    }
}
