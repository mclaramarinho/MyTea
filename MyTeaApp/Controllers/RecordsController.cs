using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<User> _um;
        private readonly SignInManager<User> _sm;
        private bool _recordCriada = false;

        public RecordsController(ApplicationDbContext context, UserManager<User> um, SignInManager<User> sm)
        {
            _context = context;
            _um = um;
            _sm = sm;
        }


        [Authorize]
        public async Task<IActionResult> Create(string? startDate, int? uid)
        {
            RecordVM vm = new RecordVM();

            DateTime date = DateTime.Now;

            if (startDate != null)
            {
                date = DateTime.ParseExact(startDate.Substring(0, 19), "yyyy-MM-ddTHH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            }

            date = new DateTime(date.Year, date.Month, firstDay);

            User userLog = await _um.FindByEmailAsync(User.Identity.Name);
            ViewData["userSelected"] = null;
            if (uid != null)
            {
                IList<string> userRoleFromParam = await _um.GetRolesAsync(userLog);

                if (userRoleFromParam[0] == "Admin")
                {
                    userLog = await _context.Users.FirstAsync(u => u.UserID == uid);
                }

                ViewData["userSelected"] = uid;

            }


            // TODO - pegar id do user


            Record? existingRecord = null;

            // TODO - procurar no banco de dados os records cuja startDate e userid sejam os procurados
            if (_context.Records.Count() > 0)
            {
                existingRecord = await _context.Records.FirstOrDefaultAsync(r => (r.StartDate == date) && r.User.Id == vm.user.Id);
            }


            // TODO - se achar algo no banco, preencher a view model com todas as informações necessárias para preencher o forms 
            if (existingRecord != null)
            {
                vm.ExistingRecord = existingRecord;

                List<RecordFraction> rf = await _context.RecordFraction.ToListAsync();

                vm.ExistingRecord.RecordFraction = rf.FindAll(f => f.RecordID == existingRecord.RecordID);


            }
            vm.WBS = _getWbsSelectList();

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ICollection<float?> hours, ICollection<DateTime> dates, ICollection<string> wbs, string email, RecordVM vm)
        {
            User user = await _um.FindByEmailAsync(User.Identity.Name);

            if (email != null)
            {
                IList<string> userRoleFromParam = await _um.GetRolesAsync(user);

                if (userRoleFromParam[0] == "Admin")
                {
                    user = await _context.Users.FirstAsync(u => u.Email == email);
                }

            }

            Record record = new Record()
            {
                TotalHoursRecord = hours.Sum().Value,
                User = user,
                StartDate = dates.ElementAt(0),
                SelectedWbs = wbs.ToList()
            };

            _context.Records.Add(record);
            await _context.SaveChangesAsync();

            int recordId = record.RecordID;

            for (int linha = 0; linha < 4; linha++)
            {
                WBS w = await _context.WBS.FirstOrDefaultAsync(w => w.WbsCod == wbs.ElementAt(linha));

                for (int col = 0; col < daysInFortnight; col++)
                {
                    if (hours.ElementAt((daysInFortnight * linha) + col) != null)
                    {
                        RecordFraction rf = new RecordFraction()
                        {
                            Record = record,
                            RecordDate = dates.ElementAt((daysInFortnight * linha) + col),
                            TotalHoursFraction = hours.ElementAt((daysInFortnight * linha) + col).Value,
                            Wbs = w
                        };

                        _context.RecordFraction.Add(rf);
                        await _context.SaveChangesAsync();

                        Record relatedRecord = await _context.Records.FirstAsync(r => r.RecordID == recordId);
                        relatedRecord.RecordFraction.Add(rf);
                        _context.Records.Update(relatedRecord);
                        await _context.SaveChangesAsync();

                        _recordCriada = true;
                        TempData["ToasterType"] = !_recordCriada ? "error" : "success";
                    }
                }
            }

            vm.WBS = _getWbsSelectList();
            return View(vm);
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
        private async Task<List<RecordFraction>> _GetNewRecordData(int rid, Record record, ICollection<float?> hours, ICollection<DateTime> dates, ICollection<string> wbs, bool isInEditMode = false)
        {
            if (isInEditMode)
        {
                List<RecordFraction> existingFractions = _context.RecordFraction.Where(f => f.RecordID == rid).ToList();
                foreach (RecordFraction fraction in existingFractions)
            {
                    _context.RecordFraction.Remove(fraction);
                    await _context.SaveChangesAsync();
                }
            }

            List<RecordFraction> fractions = new List<RecordFraction>();

            int daysInFortnight = dates.Count / 4;

            for (int linha = 0; linha < 4; linha++)
        {
                WBS? w = await _context.WBS.FirstOrDefaultAsync(w => w.WbsCod == wbs.ElementAt(linha));

                for (int col = 0; col < daysInFortnight; col++)
            {
                    if (hours.ElementAt((daysInFortnight * linha) + col) != null)
            {
                        RecordFraction rf = new RecordFraction()
                {
                            Record = record,
                            RecordDate = dates.ElementAt((daysInFortnight * linha) + col),
                            TotalHoursFraction = hours.ElementAt((daysInFortnight * linha) + col).Value,
                            Wbs = w
                        };

                        _context.RecordFraction.Add(rf);
                        await _context.SaveChangesAsync();


                        fractions.Add(rf);
                    }
                }
            }
            return fractions;
        }

    }
}