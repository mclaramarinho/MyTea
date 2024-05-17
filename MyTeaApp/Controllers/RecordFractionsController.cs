using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyTeaApp.Data;
using MyTeaApp.Models;

namespace MyTeaApp.Controllers
{
    public class RecordFractionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RecordFractionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: RecordFractions
        public async Task<IActionResult> Index()
        {
            return View(await _context.RecordFraction.ToListAsync());
        }

        // GET: RecordFractions/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recordFraction = await _context.RecordFraction
                .FirstOrDefaultAsync(m => m.RecordFractionID == id);
            if (recordFraction == null)
            {
                return NotFound();
            }

            return View(recordFraction);
        }

        // GET: RecordFractions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RecordFractions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RecordFractionID,RecordDate,TotalHoursFraction")] RecordFraction recordFraction)
        {
            if (ModelState.IsValid)
            {
                recordFraction.RecordFractionID = Guid.NewGuid();
                _context.Add(recordFraction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(recordFraction);
        }

        // GET: RecordFractions/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recordFraction = await _context.RecordFraction.FindAsync(id);
            if (recordFraction == null)
            {
                return NotFound();
            }
            return View(recordFraction);
        }

        // POST: RecordFractions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("RecordFractionID,RecordDate,TotalHoursFraction")] RecordFraction recordFraction)
        {
            if (id != recordFraction.RecordFractionID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(recordFraction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecordFractionExists(recordFraction.RecordFractionID))
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
            return View(recordFraction);
        }

        // GET: RecordFractions/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recordFraction = await _context.RecordFraction
                .FirstOrDefaultAsync(m => m.RecordFractionID == id);
            if (recordFraction == null)
            {
                return NotFound();
            }

            return View(recordFraction);
        }

        // POST: RecordFractions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var recordFraction = await _context.RecordFraction.FindAsync(id);
            if (recordFraction != null)
            {
                _context.RecordFraction.Remove(recordFraction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RecordFractionExists(Guid id)
        {
            return _context.RecordFraction.Any(e => e.RecordFractionID == id);
        }
    }
}
