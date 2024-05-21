using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyTeaApp.Data;
using MyTeaApp.Models;
using MyTeaApp.Models.ViewModels;

namespace MyTeaApp.Controllers
{
    public class RecordsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RecordsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Records
        public async Task<IActionResult> Index()
        {
            return View(await _context.Records.ToListAsync());
        }

        // GET: Records/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @record = await _context.Records
                .FirstOrDefaultAsync(m => m.RecordID == id);
            if (@record == null)
            {
                return NotFound();
            }

            return View(@record);
        }

        // GET: Records/Create
        public async Task<IActionResult> Create()
        {
            RecordVM vm = new RecordVM();
            vm.RecordFractions = new List<RecordFraction>();

            // TODO - puxar as informações do banco de dados para montar o select
            vm.WBS = new List<SelectListItem>()
            {
                new SelectListItem { Text="Ferias", Value="1" },
                new SelectListItem { Text="Day-off", Value="2" },
                new SelectListItem { Text="Desenvolvimento", Value="3" },
            };
           
           
            for (int i = 0; i < 60; i++)
            {
                vm.RecordFractions.Add(
                    new RecordFraction()
                  );
            }
            return View(vm);
        }
        [HttpPost]
        public IActionResult Create(RecordVM vm)
        {
            // TODO - Primeiro cria a record para então criar as fractins e indicar a qual record ela pertence
            Record r = new Record()
            {
                //User = colocar o user logado

            };

            foreach (var fraction in vm.RecordFractions)
            {
                var temp = new RecordFraction()
                {

                };
            }
            return View(vm);
        }

        // GET: Records/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @record = await _context.Records.FindAsync(id);
            if (@record == null)
            {
                return NotFound();
            }
            return View(@record);
        }

        // POST: Records/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RecordID,TotalHoursRecord")] Record @record)
        {
            if (id != @record.RecordID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@record);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecordExists(@record.RecordID))
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
            return View(@record);
        }

        // GET: Records/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @record = await _context.Records
                .FirstOrDefaultAsync(m => m.RecordID == id);
            if (@record == null)
            {
                return NotFound();
            }

            return View(@record);
        }

        // POST: Records/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @record = await _context.Records.FindAsync(id);
            if (@record != null)
            {
                _context.Records.Remove(@record);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RecordExists(int id)
        {
            return _context.Records.Any(e => e.RecordID == id);
        }
    }
}
