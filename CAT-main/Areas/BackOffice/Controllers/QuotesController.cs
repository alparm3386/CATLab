﻿using AutoMapper;
using CAT.Areas.BackOffice.Models.ViewModels;
using CAT.Data;
using CAT.Enums;
using CAT.Helpers;
using CAT.Infrastructure;
using CAT.Models.Common;
using CAT.Models.Entities.Main;
using CAT.Models.ViewModels;
using CAT.Services.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        private readonly ILanguageService _languageService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public QuotesController(IConfiguration configuration, IQuoteService quoteService, ILanguageService languageService,
            IDocumentService documentService, IOrderService orderService, IMapper mapper, ILogger<JobService> logger)
        {
            _configuration = configuration;
            _quoteService = quoteService;
            _documentService = documentService;
            _orderService = orderService;
            _languageService = languageService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(int? pageNumber)
        {
            try
            {
                var storedQuotes = _quoteService.GetStoredQuotes(DateTime.MinValue, DateTime.MaxValue);

                int pageSize = 10;
                pageNumber = pageNumber ?? 1;
                var paginatedStoredQuotes = await PaginatedList<StoredQuote>.CreateAsync(storedQuotes, (int)pageNumber, pageSize);

                return View(paginatedStoredQuotes);
            }
            catch (Exception ex)
            {
                // set error message here that is displayed in the view
                if (ex is CATException)
                    ViewData["ErrorMessage"] = ex.Message;
                else
                    ViewData["ErrorMessage"] = "There was an error processing your request. Please try again later.";
                // Optionally log the error: _logger.LogError(ex, "Error message here");
            }

            return View(new PaginatedList<StoredQuote>(new List<StoredQuote>(), 0, 0, 1));
        }

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
                            var clientId = 1;
                            if (model.StoredQuoteId <= 0)
                            {
                                var stroedQuote = await _quoteService.CreateStoredQuoteAsync(clientId);
                                storedQuoteId = stroedQuote.Id;
                            }

                            int idFilter = -1;
                            var document = await _documentService.CreateTempDocumentAsync(model.FileToUpload!, DocumentType.Original, idFilter);
                            //create the quote
                            var quotes = await _quoteService.CreateTempQuotesAsync(storedQuoteId, 1, model.SourceLanguage,
                                model.TargetLanguages!.ToArray(), model.Speciality, model.Service, document.Id, model.ClientReview);

                            //scope.Complete();
                        }

                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError(string.Empty, "An error occurred while calculating the quote. Please try again.");
                        return View("Create", model);
                    }

                    return RedirectToAction("StoredQuoteDetails", new { id = storedQuoteId });
                default:
                    return View("Create", model);
            }
        }

        public IActionResult Create(int? id)
        {
            var storedQuoteId = id ?? -1;
            var createQuoteViewModel = new CreateQuoteViewModel();
            createQuoteViewModel.StoredQuoteId = storedQuoteId;

            return View(createQuoteViewModel);
        }

        public async Task<IActionResult> StoredQuoteDetails(int? id)
        {
            try
            {
                var storedQuoteId = id ?? -1;
                var storedQuote = await _quoteService.GetStoredQuoteAsync(storedQuoteId, true);

                if (storedQuote == null)
                    throw new Exception("Stored quote not found.");

                //set the ViewData
                var languages = await _languageService.GetLanguages();
                ViewData["Languages"] = languages.ToDictionary(l => l.Key, l => l.Value.Name);
                ViewData["Specialities"] = EnumHelper.EnumToDisplayNamesDictionary<Speciality>();

                return View(storedQuote);
            }
            catch (Exception ex)
            {
                // set error message here that is displayed in the view
                if (ex is CATException)
                    ViewData["ErrorMessage"] = ex.Message;
                else
                    ViewData["ErrorMessage"] = "There was an error processing your request. Please try again later.";
                // Optionally log the error: _logger.LogError(ex, "Error message here");

                return View(new StoredQuote());
            }
        }

        [HttpPost]
        public async Task<IActionResult> LaunchStoredQuote(int id)
        {
            await _orderService.LaunchStoredQuotesAsync(id);

            return RedirectToAction("StoredQuoteDetails", new { id });
        }


        // GET: BackOffice/Orders/Delete/5
        public async Task<IActionResult> DeleteStoredQuote(int? id)
        {
            try
            {
                var storedQuoteId = id ?? -1;
                var storedQuote = await _quoteService.GetStoredQuoteAsync(storedQuoteId, true);

                //set the lists
                var languages = await _languageService.GetLanguages();
                ViewData["Languages"] = languages.ToDictionary(l => l.Key, l => l.Value.Name);
                ViewData["Specialities"] = EnumHelper.EnumToDisplayNamesDictionary<Speciality>();

                if (storedQuote == null)
                    throw new CATException("Stored quote not found.");

                return View(storedQuote);
            }
            catch (Exception ex)
            {
                // set error message here that is displayed in the view
                if (ex is CATException)
                    ViewData["ErrorMessage"] = ex.Message;
                else
                    ViewData["ErrorMessage"] = "There was an error processing your request. Please try again later.";
                // Optionally log the error: _logger.LogError(ex, "Error message here");

                return View(new StoredQuote());
            }
        }

        // POST: BackOffice/Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStoredQuoteConfirmed(int? id)
        {
            var storedQuote = new StoredQuote();
            try
            {
                var storedQuoteId = id ?? -1;
                storedQuote = await _quoteService.GetStoredQuoteAsync(storedQuoteId, false);
                if (storedQuote == null)
                    throw new CATException("Stored quote not found.");
                await _quoteService.DeleteStoredQuoteAsync(storedQuote);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // set error message here that is displayed in the view
                if (ex is CATException)
                    ViewData["ErrorMessage"] = ex.Message;
                else
                    ViewData["ErrorMessage"] = "There was an error processing your request. Please try again later.";
                // Optionally log the error: _logger.LogError(ex, "Error message here");

                return View("DeleteStoredQuote", storedQuote);
            }
        }
    }
}
