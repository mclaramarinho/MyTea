using Microsoft.AspNetCore.Mvc;
using MyTeaApp.Models;
using MyTeaApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyTeaApp.Data;

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

            DateTime date = _GetDateToShowRecords(startDate);

            int firstDay = _GetFirstDayOfFortnight(date);

            date = new DateTime(date.Year, date.Month, firstDay);

            User userToShow = await _um.FindByEmailAsync(User.Identity.Name);
            ViewData["userSelected"] = null;
            if (uid != null)
            {
                if (await _IsAdmin() || uid == userToShow.UserID)
                {
                    userToShow = await _context.Users.FirstAsync(u => u.UserID == uid);
                }
                else
                {
                    return RedirectToAction("Logout", "Account");
                }

                ViewData["userSelected"] = uid;

            }
            vm.user = userToShow;


            vm.ExistingRecord = null;

            //if (_context.Records.Count() > 0)
            if (_context.Records.Any())
            {
                vm.ExistingRecord = await _context.Records.FirstOrDefaultAsync(r => (r.StartDate == date) && r.User.Id == vm.user.Id);
            }


            if (vm.ExistingRecord != null)
            {
                vm.ExistingRecord.RecordFraction = await _GetFractionsFromRecord(vm.ExistingRecord.RecordID);
            }
            vm.WBS = _getWbsSelectList();

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(int? rid, ICollection<float?> hours, ICollection<DateTime> dates, ICollection<string> wbs, string email, RecordVM vm, bool isInEditMode = false)
        {
            User loggedUser = await _um.FindByEmailAsync(User.Identity.Name);
            bool isLoggedUserAdmin = await _IsAdmin();

            User user = loggedUser;


            int? userToPersist = null;

            string canContinueCreate = _CanContinueCreateAction(email, isLoggedUserAdmin, loggedUser.Email, dates);

            if (canContinueCreate == "view")
            {
                return View(vm);
            }
            else if (canContinueCreate == "logout")
            {
                return RedirectToAction("Logout", "Account");
            }
            else if (canContinueCreate == "continue")
            {
                user = await _um.FindByEmailAsync(email);
                userToPersist = user.UserID;
            }

            if (isInEditMode == true)
            {
                if (rid != null)
                {
                    var wasEdited = await EditRecord((int)rid, hours, dates, wbs, vm);
                    return wasEdited ? RedirectToAction("Create", new { uid = userToPersist, startDate = dates.ElementAt(0).ToString("yyyy-MM-dd") }) : View(vm);
                }
                return View(vm);
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

            record.RecordFraction = await _GetNewRecordData(recordId, record, hours, dates, wbs, false);

            if (record.RecordFraction.Count > 0)
            {
                _recordCriada = true;
                TempData["ToasterType"] = !_recordCriada ? "error" : "success";
            }


            vm.WBS = _getWbsSelectList();
            return RedirectToAction("Create", new { uid = userToPersist, startDate = dates.ElementAt(0).ToString("yyyy-MM-dd") });
        }

        public async Task<bool> EditRecord(int rid, ICollection<float?> hours, ICollection<DateTime> dates, ICollection<string> wbs, RecordVM vm)
        {
            vm.WBS = _getWbsSelectList();

            Record? existingRecord = await _context.Records.FirstOrDefaultAsync(rec => rec.RecordID == rid);


            if (existingRecord == null)
            {
                return false;
            }
            var fractions = await _GetNewRecordData(rid, existingRecord, hours, dates, wbs, true);
            existingRecord.SelectedWbs = wbs.ToList();

            //existingRecord.RecordFraction = fractions;

            _context.Records.Update(existingRecord);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IActionResult> Delete(int? id, string? startDate, int? userId)
        {

            if (id == null || !_context.Records.Any(r => r.RecordID == id))
            {
                return RedirectToAction("Create", new { uid = userId, startDate = startDate });
            }

            Record record = await _context.Records.FirstAsync(r => r.RecordID == id);
            _context.Records.Remove(record);
            await _context.SaveChangesAsync();

            return RedirectToAction("Create", new { uid = userId, startDate = startDate });
        }

        // ------------ UTILITIES ----------
        /// <summary>
        /// 
        /// Retrieves all the WBS from database and returns a List of SelectListItem
        /// </summary>
        /// <returns>List with the SelectListItems for each WBS</returns>

        private List<SelectListItem> _getWbsSelectList()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            var itemsFromDatabase = _context.WBS.ToList();
            selectListItems.Add(new SelectListItem
            {
                Text = "Select charge code",
                Value = "-1"
            });
            itemsFromDatabase.ForEach(i =>
            {
                selectListItems.Add(new SelectListItem
                {
                    Text = i.WbsName + " - " + i.WbsCod,
                    Value = i.WbsCod,
                });
            });
            return selectListItems;
        }

        private async Task<bool> _IsAdmin()
        {
            User userLog = await _um.FindByEmailAsync(User.Identity.Name);
            IList<string> userRoles = await _um.GetRolesAsync(userLog);
            return userRoles[0] == "Admin";
        }

        private int _GetFirstDayOfFortnight(DateTime date)
        {
            int firstDay = 1;
            if (date.Day > 15)
            {
                firstDay = 16;
            }

            return firstDay;
        }

        private DateTime _GetDateToShowRecords(string? startDate)
        {
            DateTime date = DateTime.Now;

            if (startDate != null)
            {
                date = DateTime.ParseExact(startDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            }
            return date;
        }

        private async Task<List<RecordFraction>> _GetFractionsFromRecord(int RecordID)
        {
            List<RecordFraction> rf = await _context.RecordFraction.ToListAsync();
            List<RecordFraction> response = rf.FindAll(f => f.RecordID == RecordID);

            return response;
        }
        private string _CanContinueCreateAction(string email, bool isLoggedUserAdmin, string loggedUserEmail, ICollection<DateTime> dates)
        {
            // Returns: "view" (can't continue and should return the view) || "logout" (can't continue and should return to logout action) || "continue" (can proceed)  

            string response = "view"; // can't continue and should return 

            // 1 - find out if the email belongs to an user other than the person that is creating
            if (loggedUserEmail != email)
            {
                // if true, find out if the current logged user is admin
                if (!isLoggedUserAdmin)
                {
                    // if is not admin 
                    response = "logout";
                }
                else
                {
                    // if is admin  
                    response = "continue";
                }
            }
            else
            {
                // if email from param is equal to the logged user

                if (isLoggedUserAdmin)
                {
                    // if user is admin
                    response = "continue";
                }
                else
                {
                    // if user is not admin, verify if today is in selected fortnight
                    DateTime today = DateTime.Today;
                    DateTime selectedFortnightStartDate = dates.First().Date;
                    DateTime selectedFortnightEndDate = dates.Last().Date;

                    if (today >= selectedFortnightStartDate && today <= selectedFortnightEndDate)
                    {
                        response = "continue";
                    }
                }


            }

            return response;
        }
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