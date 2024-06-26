﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyTeaApp.Data;
using MyTeaApp.Models;

namespace MyTeaApp.Controllers
{
    public class WBSController : Controller
    {
        private readonly ApplicationDbContext _context;
        private bool _contextModificado = false;
        private Random _random = new Random();


        public WBSController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: WBS
        public async Task<IActionResult> Index(string? filtroWbs)
        {
            var wbs = from w in _context.WBS
                      select w;

            if (!String.IsNullOrEmpty(filtroWbs))
            {
                wbs = wbs.Where(w => w.WbsCod.Contains(filtroWbs) || w.WbsName.Contains(filtroWbs));
            }

            TempData["SearchString"] = filtroWbs;

            return View(await wbs.ToListAsync());
        }

        // GET: WBS/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var wBS = await _context.WBS
                .FirstOrDefaultAsync(m => m.WbsId == id);
            if (wBS == null)
            {
                return NotFound();
            }

            return View(wBS);
        }

        // GET: WBS/Create
        [Authorize(Policy = "RequireAdmin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: WBS/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Create([Bind("WbsId,WbsName,WbsCod,Description,IsChargeable")] WBS wBS)
        {
            if (ModelState.IsValid)
            {
                var existingWBS = false;
                if (!wBS.WbsCod.IsNullOrEmpty())
                {
                    existingWBS = _context.WBS.Any(w => w.WbsCod == wBS.WbsCod);
                }

                if(existingWBS || wBS.WbsCod.IsNullOrEmpty())
                {
                    do
                    {
                        wBS.WbsCod = "WBS" + _random.Next(9999999).ToString();
                    } while (_context.WBS.Any(w => w.WbsCod == wBS.WbsCod));
                }

                TempData["ToasterType"] = existingWBS ? "warning" : "success";
                TempData["ToasterMessage"] = existingWBS ? "We created another wbs code. The one you informed was already taken." : "success";

                _context.Add(wBS);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ToasterType"] = "error";


                return View(wBS);
            }
        }

        // GET: WBS/Edit/5
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var wBS = await _context.WBS.FindAsync(id);
            if (wBS == null)
            {
                return NotFound();
            }
            return View(wBS);
        }

        // POST: WBS/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Edit(int id, [Bind("WbsId,WbsName,WbsCod,Description,IsChargeable")] WBS wBS)
        {
            if (id != wBS.WbsId)
            {
                _contextModificado = false;
                TempData["ToasterType"] = !_contextModificado ? "error" : "success";
                return NotFound();
                
            }

            if (ModelState.IsValid)
            {
                var existingWBS = _context.WBS.Any(w => w.WbsCod == wBS.WbsCod);

                if (existingWBS)
                {
                    ModelState.AddModelError("WBS", "Já existe uma WBS com esse código");
                    _contextModificado = false;
                    TempData["ToasterType"] = !_contextModificado ? "error" : "success";
                    return RedirectToAction(nameof(Index));

                }

                try
                {
                    _context.Update(wBS);
                    _contextModificado = true;
                    TempData["ToasterType"] = !_contextModificado ? "error" : "success";
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WBSExists(wBS.WbsId))
                    {
                        _contextModificado = false;
                        TempData["ToasterType"] = !_contextModificado ? "error" : "success";
                        return NotFound();

                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(wBS);
        }

        // GET: WBS/Delete/5
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var wBS = await _context.WBS
                .FirstOrDefaultAsync(m => m.WbsId == id);
            if (wBS == null)
            {
                return NotFound();
            }

            return View(wBS);
        }

        // POST: WBS/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var wBS = await _context.WBS.FindAsync(id);
            if (wBS != null)
            {
                _context.WBS.Remove(wBS);
                await _context.SaveChangesAsync();
                TempData["ToasterType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ToasterType"] = "error";
                return View(id);
            }
            
        }

        private bool WBSExists(int id)
        {
            return _context.WBS.Any(e => e.WbsId == id);
        }
    }
}
