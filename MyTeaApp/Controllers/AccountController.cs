﻿using Microsoft.AspNetCore.Identity;
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

            var result = await _userManager.CreateAsync(user, vm.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, vm.RoleName);

                await _signInManager.PasswordSignInAsync(user.UserName, vm.Password, false, false);
                
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
                return RedirectToAction("Index", "Home");
            }


            ModelState.AddModelError("", "Login failed");

            return View(vm);
        }
        // LOGIN END ---------------------------------------------------------------------------------------------------------------------------------------------------------





        // LOGOUT ---------------------------------------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
        // LOGOUT END ---------------------------------------------------------------------------------------------------





        // UTILITIES ---------------------------------------------------------------------------------------------------
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