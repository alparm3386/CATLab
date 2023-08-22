using AutoMapper;
using CAT.Data;
using CAT.Models.ViewModels;
using CAT.Services.CAT;
using Microsoft.AspNetCore.Mvc;

namespace CAT.Controllers.Mvc
{
    public class QuoteCalculatorController : Controller
    {
        private readonly DbContextContainer _dbContextContainer;
        private readonly IConfiguration _configuration;
        private readonly CATConnector _catConnector;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public QuoteCalculatorController(DbContextContainer dbContextContainer, IConfiguration configuration, CATConnector catConnector,
            IMapper mapper, ILogger<JobService> logger)
        {
            _dbContextContainer = dbContextContainer;
            _configuration = configuration;
            _catConnector = catConnector;
            _logger = logger;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            return View(new QuoteCalculatorViewModel());
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
                    // Your logic for Calculate Quote
                    return RedirectToAction("QuoteDetails"); // or wherever you want to redirect
                default:
                    return View("Index", model);
            }
        }

        public async Task<IActionResult> QuoteDetails(int id)
        {
            var quote = await _dbContextContainer.MainContext.Quotes.FindAsync(id);
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
