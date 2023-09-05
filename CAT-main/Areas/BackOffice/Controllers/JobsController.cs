using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using CAT.Data;
using CAT.Areas.Identity.Data;
using CAT.Helpers;
using CAT.Models.Entities.Main;
using CAT.Enums;
using CAT.Models.ViewModels;
using CAT.Services.Common;
using AutoMapper;
using Task = CAT.Enums.Task;
using CAT.Areas.BackOffice.Models.ViewModels;

namespace CAT.Areas.BackOffice.Controllers
{
    [Area("BackOffice")]
    public class JobsController : Controller
    {
        private readonly DbContextContainer _dbContextContainer;
        private readonly IConfiguration _configuration;
        private readonly JobService _jobService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public JobsController(DbContextContainer dbContextContainer, MainDbContext mainDbContext, TranslationUnitsDbContext translationUnitsDbContext,
            IConfiguration configuration, JobService jobService, IMapper mapper, ILogger<JobService> logger)
        {
            _dbContextContainer = dbContextContainer;
            _configuration = configuration;
            _jobService = jobService;
            _logger = logger;
            _mapper = mapper;
        }

        // GET: Jobs
        public async Task<IActionResult> Index()
        {
            try
            {
                // Check if TempData contains the ErrorMessage
                if (TempData.ContainsKey("ErrorMessage"))
                {
                    // Retrieve the error message from TempData and store it in a ViewBag
                    ViewBag.ErrorMessage = TempData["ErrorMessage"] as string;

                    // Clear the TempData entry to prevent the message from persisting across additional requests
                    TempData.Remove("ErrorMessage");
                }

                var jobsViewModels = await (from job in _dbContextContainer.MainContext.Jobs
                                            join document in _dbContextContainer.MainContext.Documents on job.SourceDocumentId equals document.Id
                                            select new JobViewModel
                                            {
                                                Id = job.Id,
                                                Analysis = "",
                                                DateCreated = job.Order!.DateCreated,
                                                DateProcessed = job.DateProcessed,
                                                Fee = job.Quote!.Fee,
                                                OriginalFileName = document.OriginalFileName
                                            }).ToListAsync();

                return View(jobsViewModels);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        // GET: Jobs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _dbContextContainer.MainContext.Jobs == null)
            {
                return NotFound();
            }

            var jobsViewModels = await (from job in _dbContextContainer.MainContext.Jobs
                                        join document in _dbContextContainer.MainContext.Documents on job.SourceDocumentId equals document.Id
                                        where job.Id == id
                                        select new JobViewModel
                                        {
                                            Id = job.Id,
                                            Analysis = "",
                                            DateCreated = job.Order!.DateCreated,
                                            DateProcessed = job.DateProcessed,
                                            Fee = job.Quote!.Fee,
                                            OriginalFileName = document.OriginalFileName
                                        }).FirstOrDefaultAsync();

            if (jobsViewModels == null)
            {
                return NotFound();
            }

            return View(jobsViewModels);
        }

        // GET: Jobs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Jobs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile file, IFormFile fileFilter, string sourceLang, string targetLang)
        {
            try
            {
                if (file != null && file.Length > 0)
                {
                    //save the file
                    var sourceFilesFolder = Path.Combine(_configuration["SourceFilesFolder"]!);
                    // Generate a unique file name based on the original file name
                    string fileName = FileHelper.GetUniqueFileName(file.FileName);

                    // Combine the unique file name with the server's path to create the full path
                    string filePath = Path.Combine(sourceFilesFolder, fileName);

                    // Save the file to the server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var filterName = "";
                    if (fileFilter != null && fileFilter.Length > 0)
                    {
                        var fileFiltersFolder = Path.Combine(_configuration["FileFiltersFolder"]!);
                        // Generate a unique file name based on the original file name
                        filterName = FileHelper.GetUniqueFileName(fileFilter.FileName);

                        // Combine the unique file name with the server's path to create the full path
                        string filterPath = Path.Combine(fileFiltersFolder, filterName);
                        using (var stream = new FileStream(filterPath, FileMode.Create))
                        {
                            await fileFilter.CopyToAsync(stream);
                        }
                    }

                    //create doc
                    var document = new Document()
                    {
                        OriginalFileName = file.FileName,
                        FileName = fileName,
                        DocumentType = 0,
                    };

                    // Save the job
                    _dbContextContainer.MainContext.Documents.Add(document);
                    await _dbContextContainer.MainContext.SaveChangesAsync();

                    var quote = new Quote()
                    {
                        SourceLanguage = "en",
                        TargetLanguage = "fr",
                        Fee = 10.0,
                        DateCreated = DateTime.Now,
                        Speciality = 0
                    };

                    var order = new Order()
                    {
                        DateCreated = DateTime.Now,
                        ClientId = 0,
                    };

                    var job = new Job()
                    {
                        Quote = quote,
                        Order = order,
                        SourceDocumentId = document.Id
                    };

                    //Save the order
                    _dbContextContainer.MainContext.Orders.Add(order);

                    //Save the quote
                    _dbContextContainer.MainContext.Quotes.Add(quote);

                    // Save the job
                    _dbContextContainer.MainContext.Jobs.Add(job);
                    await _dbContextContainer.MainContext.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                    throw new Exception("File is empty.");
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return View();
                //return Problem($"An error occurred: {ex.Message}", statusCode: 500);
            }
        }

        // GET: Jobs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _dbContextContainer.MainContext.Jobs == null)
            {
                return NotFound();
            }

            var job = await _dbContextContainer.MainContext.Jobs.FindAsync(id);
            if (job == null)
            {
                return NotFound();
            }
            return View(job);
        }

        // POST: Jobs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OriginalFileName,FileName,DateCreated,Analysis,Fee")] Job job)
        {
            if (id != job.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _dbContextContainer.MainContext.Update(job);
                    await _dbContextContainer.MainContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JobExists(job.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(job);
        }

        // GET: Jobs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _dbContextContainer.MainContext.Jobs == null)
            {
                return NotFound();
            }

            var job = await _dbContextContainer.MainContext.Jobs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (job == null)
            {
                return NotFound();
            }

            return View(job);
        }

        // POST: Jobs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_dbContextContainer.MainContext.Jobs == null)
            {
                return Problem("Entity set 'CATWebContext.Job'  is null.");
            }
            var job = await _dbContextContainer.MainContext.Jobs.FindAsync(id);
            if (job != null)
            {
                _dbContextContainer.MainContext.Jobs.Remove(job);
            }

            await _dbContextContainer.MainContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool JobExists(int id)
        {
            return (_dbContextContainer.MainContext.Jobs?.Any(e => e.Id == id)).GetValueOrDefault();
        }


        // GET: Jobs/Open/5
        public IActionResult OpenInOnlineEditor(int? idJob)
        {
            try
            {
                if (idJob == null)
                {
                    // Handle the case when the ID is not provided or invalid
                    // For example, return a 404 Not Found page or show an error message
                    return NotFound();
                }

                // Generate the URL
                //string onlineEditorUrl = "/online-editor?" + UrlHelper.CreateOnlineEditorUrl((int)idJob, OEMode.Admin);
                string onlineEditorUrl = UrlHelper.CreateOnlineEditorUrl(_configuration!["OnlineEditorBaseUrl"]!, (int)idJob, OEMode.Admin);


                // Redirect the request to the new URL in a new tab
                return Redirect(onlineEditorUrl);
            }
            catch (Exception ex)
            {
                // Store the error message in TempData
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";

                // Redirect back to the Index page
                return RedirectToAction(nameof(Index));
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Jobs/ProcessJob")] // explicit routing
        public async Task<IActionResult> ProcessJob(int? id)
        {
            if (id == null)
                return Problem("Invalid job ID.");

            try
            {
                await System.Threading.Tasks.Task.Run(() =>
                {
                    _jobService.ProcessJob((int)id);
                });
            }
            catch (Exception ex)
            {
                return Problem(title: "An error occurred while processing the job." + ex.Message);
            }

            return Json(new { success = true, message = "Job processed successfully." });
        }

    }
}
