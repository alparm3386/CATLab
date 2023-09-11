using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CAT.Data;
using CAT.Models.Entities.Main;

namespace CAT.Areas.BackOffice.Controllers
{
    [Area("BackOffice")]
    public class LinguistsController : Controller
    {
        private readonly MainDbContext _context;

        public LinguistsController(MainDbContext context)
        {
            _context = context;
        }

        // GET: BackOffice/Linguists
        public async Task<IActionResult> Index()
        {
            return _context.Linguists != null ?
                        View(await _context.Linguists.ToListAsync()) :
                        Problem("Entity set 'MainDbContext.Linguists'  is null.");
        }

        // GET: BackOffice/Linguists/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Linguists == null)
            {
                return NotFound();
            }

            var linguist = await _context.Linguists
                .FirstOrDefaultAsync(m => m.Id == id);
            if (linguist == null)
            {
                return NotFound();
            }

            return View(linguist);
        }

        // GET: BackOffice/Linguists/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BackOffice/Linguists/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId")] Linguist linguist)
        {
            if (ModelState.IsValid)
            {
                _context.Add(linguist);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(linguist);
        }

        // GET: BackOffice/Linguists/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Linguists == null)
            {
                return NotFound();
            }

            var linguist = await _context.Linguists.FindAsync(id);
            if (linguist == null)
            {
                return NotFound();
            }
            return View(linguist);
        }

        // POST: BackOffice/Linguists/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId")] Linguist linguist)
        {
            if (id != linguist.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(linguist);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LinguistExists(linguist.Id))
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
            return View(linguist);
        }

        // GET: BackOffice/Linguists/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Linguists == null)
            {
                return NotFound();
            }

            var linguist = await _context.Linguists
                .FirstOrDefaultAsync(m => m.Id == id);
            if (linguist == null)
            {
                return NotFound();
            }

            return View(linguist);
        }

        // POST: BackOffice/Linguists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Linguists == null)
            {
                return Problem("Entity set 'MainDbContext.Linguists'  is null.");
            }
            var linguist = await _context.Linguists.FindAsync(id);
            if (linguist != null)
            {
                _context.Linguists.Remove(linguist);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LinguistExists(int id)
        {
            return (_context.Linguists?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
