using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Update.Internal;
namespace MyTeaApp.Models.ViewModels
{
    public class EditUserVM
    {
        [Required]
        public int UserID { get; set; }

        [Required]
        public string UserDbId { get; set; }

        [Required]
        [DisplayName("Full Name")]
        public string? FullName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        [DisplayName("Departament")]
        public string DepartmentId { get; set; }
        public List<SelectListItem> Departments { get; set; }

        [Required]
        [DisplayName("Currently employed?")]
        public string IsActive { get; set; }
        public List<SelectListItem> ActiveStates { get; set; }
        
        [Required]
        [DisplayName("Role")]
        public string RoleName { get; set; }
        public List<SelectListItem> Roles { get; set; }

        public void Populate(User user, string role, List<Department> deps, List<IdentityRole> roles)
        {
            this.UserID = user.UserId;
            this.UserDbId = user.Id;
            this.FullName = user.FullName;
            this.Email = user.Email;
            this.RoleName = role;
            this.DepartmentId = user.DepartmentId.ToString();

            this.ActiveStates = new List<SelectListItem>()
            {
                new SelectListItem { Text = "Yes", Value = "yes", Selected = (user.UserActive == true)},
                new SelectListItem { Text = "No", Value = "no", Selected = (user.UserActive == false)}
            };

            this.Departments = new List<SelectListItem>();
            this.Roles = new List<SelectListItem>();
            deps.ForEach(d => Departments.Add(new SelectListItem { Text = d.DepartmentName, Value = d.DepartmentID.ToString() }));
            roles.ForEach(r => Roles.Add(new SelectListItem { Text = r.Name, Value = r.Name}));

        }
    }
}
