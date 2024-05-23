﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyTeaApp.Data;
using MyTeaApp.Models;
using MyTeaApp.Models.ViewModels;

namespace MyTeaApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        public AccountController(ApplicationDbContext db, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
        }



        [HttpGet]
        public async Task<IActionResult> Index(string? userName, DateTime? admissionDate, string? activeOnly, string? department, string? role)
        {
            List<User> users = await _db.Users.ToListAsync();

            List<UserInfoVM> viewModel = new List<UserInfoVM>();

            users = userName != null ? 
                users.FindAll(u => u.FullName.Contains(userName)) : users;

            users = (activeOnly != null && activeOnly.Equals("on")) ?
                users.FindAll(u => u.UserActive == true) : users;

            users = admissionDate.HasValue
                ? users.FindAll(u => u.AdmissionDate >= admissionDate.Value) : users;


            foreach (User user in users)
            {
                string dpt = _db.Department.FirstOrDefault(d => d.DepartmentID == user.DepartmentId).DepartmentName;
                string userRole = await _userManager.IsInRoleAsync(user, "Admin") ? "Admin"
                    : await _userManager.IsInRoleAsync(user, "Employee") ? "Employee"
                    : "Manager";

                if (role==null || role == "all" || role == userRole)
                {
                    if (department == null || department == "all" || department == dpt)
                    {
                        UserInfoVM temp = new UserInfoVM()
                        {
                            DbUserId = user.Id,
                            UserId = user.UserID,
                            FullName = user.FullName,
                            Email = user.Email,
                            AdmissionDate = user.AdmissionDate,
                            DepartmentName = dpt,
                            RoleName = userRole,
                            Records = user.Records,
                            IsActive = user.UserActive ? "Yes" : "No"
                        };
                        viewModel.Add(temp);
                    }
                }

            }

            ViewData["filter.UserName"] = userName;
            ViewData["filter.admissionDate"] = admissionDate?.ToString("yyyy-MM-dd");
            ViewData["filter.activeOnly"] = activeOnly;
            ViewData["filter.role"] = role;
            ViewData["filter.department"] = department;

            return View(viewModel);
        }


        // REGISTER ---------------------------------------------------------------------------------------------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            // Not allowed to register
            if (!_isAllowedToRegister()) 
            {
                return RedirectToAction(nameof(Login));
            }

            // Allowed to register
            RegisterVM vm = new RegisterVM();

            vm.Departments = await _populateDepartmentSelectList();

            vm.Roles = await _populateRoleSelectList();           


            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM vm)
        {
            if (!_isAllowedToRegister())
            {
                if (!_isFirstRegister())
                {
                    ModelState.AddModelError("", "An administrator is already registered. Registrations can only be made by them.");
                }
                return View(vm);
            }
            bool shouldLoginAfter = _isFirstRegister();
            Department dpt = _db.Department.First(d => d.DepartmentID == vm.DepartmentID);

            User user = new User()
            {
                UserName = vm.Email,
                Email = vm.Email,

                FullName = vm.FullName,
                AdmissionDate = vm.AdmissionDate,
                Department = dpt,
                UserActive = true
            };
            user.SetUID();
            var result = await _userManager.CreateAsync(user, vm.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, vm.RoleName);

                if (shouldLoginAfter)
                {
                    await _signInManager.PasswordSignInAsync(user.UserName, vm.Password, false, false);
                    return _redirectAfterLogin();
                }
                return RedirectToAction("Index", "Home");
            }
            return View(vm);

        }
        // REGISTER END ---------------------------------------------------------------------------------------------------------------------------------------------------------

        
        
        
        
        // LOGIN ---------------------------------------------------------------------------------------------------------------------------------------------------------
        [HttpGet]
        public IActionResult Login()
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }
            var vm = new LoginVM();
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM vm)
        {   
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Verify all the information. Something's wrong...");
                return View(vm);
            }

            var result = await _signInManager.PasswordSignInAsync(vm.Email, vm.Password, vm.RememberMe, false);

            if (result.Succeeded)
            {
                return _redirectAfterLogin();
            }

            ModelState.AddModelError("", "Login failed");

            return View(vm);
        }
        // LOGIN END ---------------------------------------------------------------------------------------------------------------------------------------------------------





        // LOGOUT ---------------------------------------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }
        // LOGOUT END ---------------------------------------------------------------------------------------------------


        // EDIT ---------------------------------------------------------------------------------------------------
        
        [HttpGet]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> EditUser(int uid) {
            User user = await _db.Users.FirstAsync(u => u.UserID == uid);
            if (user == null)
            {
                // do something if no user found
            }
            List<Department> departments = await _db.Department.ToListAsync();
            List<IdentityRole> rolesDb = await _db.Roles.ToListAsync();

            var role = await _userManager.GetRolesAsync(user);

            EditUserVM vm = new EditUserVM();
            vm.Populate(user, role[0], departments, rolesDb);

            return View(vm);
        }

        [HttpPost]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> EditUser(int uid, [Bind("UserID, FullName, Email, DepartmentId, RoleName")]EditUserVM data)
        {
            User user = await _db.Users.FirstAsync(u => u.UserID == uid);

            var role = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRoleAsync(user, role[0]);

            Department department = await _db.Department.FirstAsync(d => d.DepartmentID == int.Parse(data.DepartmentId));

            user.Department = department;
            user.FullName = data.FullName;
            user.Email = data.Email;
            user.UserName = data.Email;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return View(data);
            }
            var updateRoleResult = await _userManager.AddToRoleAsync(user, data.RoleName);
            if(!updateRoleResult.Succeeded) { 
                return View(data);
            }

            return RedirectToAction(nameof(Index));

        }
      
        // EDIT END ---------------------------------------------------------------------------------------------------



        // UTILITIES ---------------------------------------------------------------------------------------------------
        private IActionResult _redirectAfterLogin()
        {
            if (User.IsInRole("Admin") || User.IsInRole("Manager"))
            {
                return RedirectToAction("Dashboard", "Home");
            }else if(User.IsInRole("Employee"))
            {
                return RedirectToAction("Create", "Records");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private bool _isFirstRegister()
        {
            List<User> users = _db.Users.ToList();
            return users.Count == 0;
        }

        private async Task<List<SelectListItem>> _populateDepartmentSelectList()
        {
            List<SelectListItem> selectItems = new List<SelectListItem>();

            var deps = await _db.Department.ToListAsync();

            foreach (var dep in deps)
            {
                selectItems.Add(
                        new SelectListItem { Text = dep.DepartmentName, Value = dep.DepartmentID.ToString() }
                );
            }

            return selectItems;
        }

        private async Task<List<SelectListItem>> _populateRoleSelectList()
        {
            List<SelectListItem> selectItems = new List<SelectListItem>();

            if (_isFirstRegister())
            {
                selectItems.Add(
                    new SelectListItem { Text = "Administrator", Value = "Admin" }
                );
            }
            else
            {
                var roles = await _db.Roles.ToListAsync();

                foreach (var item in roles)
                {
                    selectItems.Add(
                        new SelectListItem { Text = item.Name, Value = item.Name }
                    );
                }
            }

            return selectItems;
        }
        private bool _isAllowedToRegister()
        {
            if (!_signInManager.IsSignedIn(User))
            {
                // Check if it is the first registration
                // If not
                if (!_isFirstRegister())
                {
                    // Not allowed to register
                    return false;
                }
            }
            // If someone is logged in
            else
            {
                // Check if the logged user is Admin
                // If not
                if (!User.IsInRole("Admin"))
                {
                    // Not allowed to register
                    return false;
                }
            }

            return true;
        }

    }
}
