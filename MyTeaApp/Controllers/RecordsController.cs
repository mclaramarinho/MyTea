using Microsoft.AspNetCore.Mvc;
using MyTeaApp.Models;
using MyTeaApp.Models.ViewModels;
﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyTeaApp.Data;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Collections.Generic;

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

        // ------- UTILITIES ------------

        /// <summary>
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
                Value = "-1",
                Selected = true
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