using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyTeaApp.Data;
using MyTeaApp.Models;
using MyTeaApp.Models.HelperModels.GenerateReport;
using Newtonsoft.Json;
using NuGet.Protocol;

namespace MyTeaApp.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        
        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }
        [Authorize(policy: "RequireManager")]
        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public async Task<string> GenerateReport()
        {
            Body userList = await _ReadBody(HttpContext.Request);
            List<UserInfo> response = new List<UserInfo>();

            List<User> allUsers = new List<User>();

            if(userList.Users.Count == 0)
            {
                allUsers = await _context.Users.ToListAsync();
            }
            else
            {
                foreach(int u in userList.Users)
                {
                    User? currentUser = _GetUserById(u);
                    allUsers.Add(currentUser);
                }
            }


            foreach (User currentUser in allUsers)
            {
                //User? currentUser = _GetUserById(user);
                if (currentUser == null)
                {
                    UserInfo temp = new UserInfo()
                    {
                        FullName = null,
                        Email = null,
                        DepartmentName = null,
                        ActiveUser = false,
                        TotalWorkedHours = 0,
                        MostUsedWbs = [],
                        MostUsedWbsHours = [],
                        NoWorkTime = 0
                    };

                    response.Add(temp);
                    continue;
                }

                string depName = _context.Department.First(d => d.DepartmentID == currentUser.DepartmentId).DepartmentName;

                IEnumerable<Record>? userRecords = await _GetRecordsByUser(currentUser.Id);
                if (userRecords == null)
                {
                    UserInfo temp = new UserInfo()
                    {
                        FullName = currentUser.FullName,
                        Email = currentUser.Email,
                        DepartmentName = depName,
                        ActiveUser = currentUser.UserActive,
                        TotalWorkedHours = 0,
                        MostUsedWbs = [],
                        MostUsedWbsHours = [],
                        NoWorkTime = 0
                    };

                    response.Add(temp);
                    continue;
                }

                List<RecordFraction> fractions = _GetRecordFractionsByRecordId(userRecords);

                List<string> allUsedWbs = await _GetAllUsedWbs(fractions);

                List<string> allUsedWbsDistinct = allUsedWbs.Distinct().ToList();
                List<float> allUsedWbsDistinctHours = await _GetAmountOfTimeForEachWbs(allUsedWbsDistinct, fractions);
                List<int> amountUsedWbs = _GetAmountOfDaysForEachWbs(allUsedWbsDistinct, allUsedWbs);


                List<int> amountUsedWbsOrder = amountUsedWbs.Order().ToList();
                List<float> allUsedWbsDistinctHoursOrder = allUsedWbsDistinctHours.Order().ToList();



                List<string> mostHoursWbs = _GetMostHoursWbs(allUsedWbsDistinctHoursOrder, allUsedWbsDistinctHours, allUsedWbsDistinct); ;

                List<float> mostHoursWbsTime = allUsedWbsDistinctHoursOrder;

                float notChargeableHours = _GetNonChargeableHours(mostHoursWbs, mostHoursWbsTime);

                UserInfo tempInfo = new UserInfo()
                {
                    FullName = currentUser.FullName,
                    Email = currentUser.Email,
                    DepartmentName = depName,
                    ActiveUser = currentUser.UserActive,
                    TotalWorkedHours = userRecords.Sum(r => r.TotalHoursRecord),
                    MostUsedWbs = mostHoursWbs,
                    MostUsedWbsHours = mostHoursWbsTime,
                    NoWorkTime = notChargeableHours
                };

                response.Add(tempInfo);
            }


            return response.ToJson();
        }

        
        // ----------------- UTILITIES ---------------------------------------------------------------------------------
        private async Task<Body> _ReadBody(HttpRequest httpRequest)
        {
            using var reader = new StreamReader(httpRequest.Body);
            var body = await reader.ReadToEndAsync();
            return JsonConvert.DeserializeObject<Body>(body);
        }

        private User? _GetUserById(int id)
        {
            return _context.Users.FirstOrDefault(u => u.UserId == id);
        }
        private async Task<IEnumerable<Record>>? _GetRecordsByUser(string guid)
        {
            try
            {
                if (!_context.Records.Any(r => r.User.Id == guid))
                {
                    Console.WriteLine("no record");
                    throw new Exception();
                }
                Console.WriteLine(_context.Records.Any(r => r.User.Id == guid));
                IEnumerable<Record> response = _context.Records.Where(rec => rec.User.Id == guid);
                return response;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private List<RecordFraction> _GetRecordFractionsByRecordId(IEnumerable<Record> userRecords)
        {
            List<RecordFraction> fractions = new List<RecordFraction>();

            foreach (Record rec in userRecords)
            {
                IEnumerable<RecordFraction> fs = from f in _context.RecordFraction.ToList()
                                                 where f.RecordID == rec.RecordID
                                                 select f;

                foreach (RecordFraction f in fs.ToList())
                {
                    fractions.Add(f);
                }
            }
            return fractions;
        }

        private async Task<List<string>> _GetAllUsedWbs(List<RecordFraction> fractions)
        {
            List<WBS> wbsFromDb = await _context.WBS.ToListAsync();
            List<string> allUsedWbs = new List<string>();

            foreach (RecordFraction f in fractions)
            {
                string wbsCod = wbsFromDb.First(w => w.WbsId == f.WbsID).WbsCod;
                allUsedWbs.Add(wbsCod);
            }
            return allUsedWbs;
        }

        private List<int> _GetAmountOfDaysForEachWbs(List<string> allUsedWbsDistinct, List<string> allUsedWbs)
        {
            int position = 0;
            List<int> amountUsedWbs = new List<int>();

            foreach (string w in allUsedWbsDistinct)
            {
                int count = 0;
                amountUsedWbs.Add(count);

                foreach (string wbs in allUsedWbs)
                {
                    if (wbs == w)
                    {
                        amountUsedWbs[position] = ++count;
                    }
                }
                position++;
            }

            return amountUsedWbs;
        }

        private async Task<List<float>> _GetAmountOfTimeForEachWbs(List<string> allUsedWbsDistinct, List<RecordFraction> fractions)
        {
            List<WBS> wbsFromDb = await _context.WBS.ToListAsync();
            List<float> hourPerWbs = new List<float>();

            int position = 0;
            foreach (string wbs in allUsedWbsDistinct)
            {
                float hourCount = 0;

                float fractionHourCount = fractions.Where(f => f.Wbs.WbsCod == wbs).Sum(f => f.TotalHoursFraction);
                hourCount += fractionHourCount;

                hourPerWbs.Add(hourCount);
            }

            return hourPerWbs;

        }

        private List<string> _GetMostHoursWbs(List<float> allUsedWbsDistinctHoursOrder, List<float> allUsedWbsDistinctHours, List<string> allUsedWbsDistinct)
        {
            List<string> mostHoursWbs = new List<string>();
            List<int> indexesPassed = new List<int>();

            foreach (float hour in allUsedWbsDistinctHoursOrder)
            {
                List<int> indexesWhereIsEqualToHour = new List<int>();

                for (int i = 0; i < allUsedWbsDistinctHours.Count; i++)
                {
                    if (allUsedWbsDistinctHours[i] == hour)
                    {
                        indexesWhereIsEqualToHour.Add(i);
                    }
                }

                foreach (int index in indexesWhereIsEqualToHour)
                {
                    if (!indexesPassed.Contains(index))
                    {
                        indexesPassed.Add(index);

                        mostHoursWbs.Add(allUsedWbsDistinct[index]);
                        break;
                    }
                }

            }
            return mostHoursWbs;
        }

        private float _GetNonChargeableHours(List<string> mostHoursWbs, List<float> mostHoursWbsTime)
        {
            IEnumerable<WBS> notChargeableWbs = (from w in _context.WBS.ToList()
                                                 where w.IsChargeable == false
                                                 select w).ToList();
            float notChargeableHours = 0;


            foreach (WBS w in notChargeableWbs)
            {
                int matchingIndex = mostHoursWbs.FindIndex(wbs => wbs == w.WbsCod);

                if (matchingIndex >= 0)
                {
                    notChargeableHours += mostHoursWbsTime[matchingIndex];
                }
            }

            return notChargeableHours;
        }

    }
}
