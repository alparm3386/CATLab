using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CAT.Data;
using CAT.Models.Entities.Main;

namespace CAT.Areas.ClientPortal.Controllers
{
    [Area("ClientsPortal")]
    public class ClientsPortalController : Controller
    {
        private readonly MainDbContext _context;

        public ClientsPortalController(MainDbContext context)
        {
            _context = context;
        }

        // GET: BackOffice/ClientPortal
        //[Route("[area]/[controller]/[action]")]
        [Route("ClientsPortal/Index")]
        public async Task<IActionResult> Index()
        {
            var clientId = 1;
            var mainDbContext = _context.Jobs.Include(j => j.Order).Include(j => j.Quote).Where(j => j.Order.ClientId == clientId);
            return View(await mainDbContext.ToListAsync());
        }

        // GET: BackOffice/ClientPortal/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Jobs == null)
            {
                return NotFound();
            }

            var job = await _context.Jobs
                .Include(j => j.Order)
                .Include(j => j.Quote)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (job == null)
            {
                return NotFound();
            }

            return View(job);
        }

        // GET: BackOffice/ClientPortal/Create
        public IActionResult Create()
        {
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id");
            ViewData["QuoteId"] = new SelectList(_context.Quotes, "Id", "Id");
            return View();
        }

        // POST: BackOffice/ClientPortal/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,OrderId,QuoteId,SourceDocumentId,FinalDocumentId,DateProcessed")] Job job)
        {
            if (ModelState.IsValid)
            {
                _context.Add(job);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", job.OrderId);
            ViewData["QuoteId"] = new SelectList(_context.Quotes, "Id", "Id", job.QuoteId);
            return View(job);
        }

        // GET: BackOffice/ClientPortal/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Jobs == null)
            {
                return NotFound();
            }

            var job = await _context.Jobs.FindAsync(id);
            if (job == null)
            {
                return NotFound();
            }
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", job.OrderId);
            ViewData["QuoteId"] = new SelectList(_context.Quotes, "Id", "Id", job.QuoteId);
            return View(job);
        }

        // POST: BackOffice/ClientPortal/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OrderId,QuoteId,SourceDocumentId,FinalDocumentId,DateProcessed")] Job job)
        {
            if (id != job.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(job);
                    await _context.SaveChangesAsync();
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
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", job.OrderId);
            ViewData["QuoteId"] = new SelectList(_context.Quotes, "Id", "Id", job.QuoteId);
            return View(job);
        }

        // GET: BackOffice/ClientPortal/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Jobs == null)
            {
                return NotFound();
            }

            var job = await _context.Jobs
                .Include(j => j.Order)
                .Include(j => j.Quote)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (job == null)
            {
                return NotFound();
            }

            return View(job);
        }

        // POST: BackOffice/ClientPortal/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Jobs == null)
            {
                return Problem("Entity set 'MainDbContext.Jobs'  is null.");
            }
            var job = await _context.Jobs.FindAsync(id);
            if (job != null)
            {
                _context.Jobs.Remove(job);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool JobExists(int id)
        {
            return (_context.Jobs?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
