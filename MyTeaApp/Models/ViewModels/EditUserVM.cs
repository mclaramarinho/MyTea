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
        [DisplayName("Full Name")]
        public string? FullName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        [DisplayName("Departament")]
        public string DepartmentId { get; set; }
        public List<SelectListItem> Departments { get; set; }

        [Required]
        [DisplayName("Role")]
        public string RoleName { get; set; }
        public List<SelectListItem> Roles { get; set; }

        public void Populate(User user, string role, List<Department> deps, List<IdentityRole> roles)
        {
            Console.WriteLine("User UID: " + user.UserID);
            this.UserID = user.UserID;
            this.FullName = user.FullName;
            this.Email = user.Email;
            this.RoleName = role;
            this.DepartmentId = user.DepartmentId.ToString();

            this.Departments = new List<SelectListItem>();
            this.Roles = new List<SelectListItem>();
            deps.ForEach(d => Departments.Add(new SelectListItem { Text = d.DepartmentName, Value = d.DepartmentID.ToString() }));
            roles.ForEach(r => Roles.Add(new SelectListItem { Text = r.Name, Value = r.Name}));

        }
    }
}
