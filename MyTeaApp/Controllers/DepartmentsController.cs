using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyTeaApp.Data;
using MyTeaApp.Models;

namespace MyTeaApp.Controllers
{
    public class DepartmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Departments
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Index(string searchString)
        {
            var departments = from d in _context.Department
                         select d;
            if (!string.IsNullOrEmpty(searchString))
            {
                if (int.TryParse(searchString, out int id))
                {
                    departments = departments.Where(d => d.DepartmentID == id);
                }
                else
                {
                    departments = departments.Where(d => d.DepartmentName!.Contains(searchString));
                }
            }
            return View(await departments.ToListAsync());
        }

        

        // GET: Departments/Details/5
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Department
                .FirstOrDefaultAsync(m => m.DepartmentID == id);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // GET: Departments/Create
        [Authorize(Policy = "RequireAdmin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Departments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Create([Bind("DepartmentID,DepartmentName")] Department department)
        {
            if (ModelState.IsValid)
            {
                _context.Add(department);
                await _context.SaveChangesAsync();
                TempData["ToasterType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            TempData["ToasterType"] = "error";
            return View(department);
        }

        // GET: Departments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Department.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            return View(department);
        }

        // POST: Departments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Edit(int id, [Bind("DepartmentID,DepartmentName")] Department department)
        {
            if (id != department.DepartmentID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(department);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartmentExists(department.DepartmentID))
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
            return View(department);
        }

        // GET: Departments/Delete/5
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["ToasterType"] = "error";
                return NotFound();
            }

            var department = await _context.Department
                .FirstOrDefaultAsync(m => m.DepartmentID == id);
            if (department == null)
            {
                TempData["ToasterType"] = "error";
                return NotFound();
            }
            TempData["ToasterType"] = "success";
            return View(department);
        }

        // POST: Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var department = await _context.Department.FindAsync(id);
            if (department != null)
            {
                _context.Department.Remove(department);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DepartmentExists(int id)
        {
            return _context.Department.Any(e => e.DepartmentID == id);
        }
    }
}
