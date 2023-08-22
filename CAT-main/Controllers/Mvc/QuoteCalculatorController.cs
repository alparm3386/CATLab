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
        public async Task<IActionResult> CalculateQuote(QuoteCalculatorViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Calculate the price for the uploaded file. This is a placeholder.
                //double price = CalculatePriceBasedOnFileAndCriteria(model);

                //var quote = new Quote
                //{
                //    SourceLanguage = model.SourceLanguage,
                //    TargetLanguage = model.TargetLanguage,
                //    Speciality = model.Speciality,
                //    Price = price,
                //    // Store file path if necessary
                //};

                //_dbContext.Quotes.Add(quote);
                //await _dbContext.SaveChangesAsync();

                //return RedirectToAction("QuoteDetails", new { id = quote.Id });

                return RedirectToAction("QuoteDetails", 1);
            }

            return View("Index", model);
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
