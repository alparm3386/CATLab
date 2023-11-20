﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CAT.Data;
using CAT.Models.Entities.Main;
using CAT.Helpers;
using CAT.Enums;
using Microsoft.AspNetCore.Authorization;

namespace CAT.Areas.Linguists.Controllers
{
    [Area("LinguistsPortal")]
    public class JobsController : Controller
    {
        private readonly MainDbContext _mainDbcontext;
        private readonly IConfiguration _configuration;

        public JobsController(MainDbContext context, IConfiguration configuration)
        {
            _mainDbcontext = context;
            _configuration = configuration;
        }

        // GET: BackOffice/ClientsPortal
        //[Route("[area]/[controller]/[action]")]
        [Route("LinguistsPortal/Jobs")]
        public async Task<IActionResult> Jobs()
        {
            //get the jobs
            var clientId = 1;
            var jobs = _mainDbcontext.Jobs.Include(j => j.Order).Include(j => j.Quote).Where(j => j.Order!.ClientId == clientId);

            //join into the documents table 
            var jobsWithDocuments = await (from j in jobs
                                           join d in _mainDbcontext.Documents on j.SourceDocumentId equals d.Id
                                           select new
                                           {
                                               orderId = j.OrderId,
                                               jobId = j.Id,
                                               sourceLanguage = j.Quote!.SourceLanguage,
                                               targetLanguage = j.Quote.TargetLanguage,
                                               speciality = j.Quote.Speciality,
                                               speed = j.Quote.Speed,
                                               service = j.Quote.Service,
                                               documentId = d.Id,
                                               originalFileName = d.OriginalFileName,
                                               fileName = d.FileName,
                                               words = j.Quote.Words,
                                               fee = j.Quote.Fee,
                                               workflowSteps = j.WorkflowSteps
                                           }).ToListAsync();

            var viewData = new { jobsWithDocuments, name = "aaaaa" };
            return View(viewData);
        }

        // GET: BackOffice/ClientsPortal/Details/5
        public async Task<IActionResult> JobDetails(int? id)
        {
            if (id == null || _mainDbcontext.Jobs == null)
            {
                return NotFound();
            }

            var job = await _mainDbcontext.Jobs
                .Include(j => j.Order)
                .Include(j => j.Quote)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (job == null)
            {
                return NotFound();
            }

            return View(job);
        }

        public IActionResult OpenInOnlineEditor(int? jobId)
        {
            try
            {
                if (jobId == null)
                {
                    // Handle the case when the ID is not provided or invalid
                    // For example, return a 404 Not Found page or show an error message
                    return NotFound();
                }

                // Generate the URL
                //string onlineEditorUrl = "/online-editor?" + UrlHelper.CreateOnlineEditorUrl((int)idJob, OEMode.Admin);
                string onlineEditorUrl = UrlHelper.CreateOnlineEditorUrl(_configuration!["OnlineEditorBaseUrl"]!, (int)jobId, OEMode.Admin);


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
