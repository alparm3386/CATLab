﻿using System;
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

namespace CAT.Controllers.MvcControllers
{
    public class JobsController : Controller
    {
        private readonly IdentityDbContext _identityDBContext;
        private readonly MainDbContext _mainDbContext;
        private readonly TranslationUnitsDbContext _translationUnitsDbContext;
        private readonly IConfiguration _configuration;

        public JobsController(IdentityDbContext identityDBContext, MainDbContext mainDbContext, TranslationUnitsDbContext translationUnitsDbContext,
            IConfiguration configuration)
        {
            _identityDBContext = identityDBContext;
            _mainDbContext = mainDbContext;
            _translationUnitsDbContext = translationUnitsDbContext;
            _configuration = configuration;
        }

        // GET: Jobs
        public async Task<IActionResult> Index()
        {
            // Check if TempData contains the ErrorMessage
            if (TempData.ContainsKey("ErrorMessage"))
            {
                // Retrieve the error message from TempData and store it in a ViewBag
                ViewBag.ErrorMessage = TempData["ErrorMessage"] as string;

                // Clear the TempData entry to prevent the message from persisting across additional requests
                TempData.Remove("ErrorMessage");
            }

            var jobs = await _mainDbContext.Jobs.Include(j => j.Quote).Include(j => j.Order).ToListAsync();

            var jobsViewModels = new List<JobViewModel>();
            return _mainDbContext.Jobs != null ?
                        View(jobsViewModels) :
                        Problem("Entity set 'CATWebContext.Job'  is null.");
        }

        // GET: Jobs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _mainDbContext.Jobs == null)
            {
                return NotFound();
            }

            var job = await _mainDbContext.Jobs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (job == null)
            {
                return NotFound();
            }

            return View(job);
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
                    var sourceFilesFolder = Path.Combine(_configuration["SourceFilesFolder"]);
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
                        var fileFiltersFolder = Path.Combine(_configuration["FileFiltersFolder"]);
                        // Generate a unique file name based on the original file name
                        filterName = FileHelper.GetUniqueFileName(fileFilter.FileName);

                        // Combine the unique file name with the server's path to create the full path
                        string filterPath = Path.Combine(fileFiltersFolder, filterName);
                        using (var stream = new FileStream(filterPath, FileMode.Create))
                        {
                            await fileFilter.CopyToAsync(stream);
                        }
                    }

                    var quote = new Quote()
                    {
                        SourceLanguage = "en",
                        TargetLanguage = "fr",
                        Fee = 10.0,
                        DateCreated = DateTime.Now,
                        Specility = 0
                    };

                    var job = new Job()
                    {
                        Quote = quote
                    };

                    var order = new Order()
                    {
                        DateCreated = DateTime.Now,
                        ClientId = 0,
                    };

                    order.Jobs.Add(job);


                    // Save the job object to the database
                    _mainDbContext.Jobs.Add(job);
                    await _mainDbContext.SaveChangesAsync();
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
            if (id == null || _mainDbContext.Jobs == null)
            {
                return NotFound();
            }

            var job = await _mainDbContext.Jobs.FindAsync(id);
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
                    _mainDbContext.Update(job);
                    await _mainDbContext.SaveChangesAsync();
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
            if (id == null || _mainDbContext.Jobs == null)
            {
                return NotFound();
            }

            var job = await _mainDbContext.Jobs
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
            if (_mainDbContext.Jobs == null)
            {
                return Problem("Entity set 'CATWebContext.Job'  is null.");
            }
            var job = await _mainDbContext.Jobs.FindAsync(id);
            if (job != null)
            {
                _mainDbContext.Jobs.Remove(job);
            }

            await _mainDbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool JobExists(int id)
        {
            return (_mainDbContext.Jobs?.Any(e => e.Id == id)).GetValueOrDefault();
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
                string onlineEditorUrl = UrlHelper.CreateOnlineEditorUrl((int)idJob, OEMode.Admin);


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
    }
}