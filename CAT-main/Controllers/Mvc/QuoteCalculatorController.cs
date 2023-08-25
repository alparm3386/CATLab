using AutoMapper;
using CAT.Data;
using CAT.Enums;
using CAT.Helpers;
using CAT.Models.Common;
using CAT.Models.Entities.Main;
using CAT.Models.ViewModels;
using CAT.Services.Common;
using Microsoft.AspNetCore.Mvc;
using System.Transactions;

namespace CAT.Controllers.Mvc
{
    public class QuoteCalculatorController : Controller
    {
        private readonly DbContextContainer _dbContextContainer;
        private readonly IConfiguration _configuration;
        private readonly QuoteService _quoteService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public QuoteCalculatorController(DbContextContainer dbContextContainer, IConfiguration configuration, QuoteService quoteService,
            IMapper mapper, ILogger<JobService> logger)
        {
            _dbContextContainer = dbContextContainer;
            _configuration = configuration;
            _quoteService = quoteService;
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
                    try
                    {
                        //using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)) //MSDTC
                        //using (var transaction = context.Database.BeginTransaction())
                        {
                            //save the file
                            var sourceFilesFolder = Path.Combine(_configuration["SourceFilesFolder"]!);
                            // Generate a unique file name based on the original file name
                            string fileName = FileHelper.GetUniqueFileName(model!.FileToUpload!.FileName);

                            // Combine the unique file name with the server's path to create the full path
                            string filePath = Path.Combine(sourceFilesFolder, fileName);

                            // Save the file to the server
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await model!.FileToUpload!.CopyToAsync(stream);
                            }

                            //create the document
                            var document = new Document()
                            {
                                DocumentType = (int)DocumentType.Original,
                                FileName = Path.GetFileNameWithoutExtension(filePath),
                                OriginalFileName = fileName
                            };

                            await _dbContextContainer.MainContext.Documents.AddAsync(document);
                            await _dbContextContainer.MainContext.SaveChangesAsync();

                            //create the quote
                            _quoteService.CreateQuote(1, new LocaleId(model.SourceLanguage!), new LocaleId[] { new LocaleId(model.TargetLanguage!) },
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
