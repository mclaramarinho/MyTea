using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MyTeaApp.Models.ViewModels
{
    public class RegisterVM
    {
        [Required]
        [DisplayName("Full Name")]
        [DataType(DataType.Text)]
        public string FullName { get; set; }


        [Required]
        [DataType(DataType.EmailAddress)]
        [DisplayName("Email Address")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8), MaxLength(20)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayName("Admission Date")]
        [DisplayFormat(DataFormatString = "{0: dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime AdmissionDate { get; set; }

        [Required]
        [DisplayName("Department")]
        public int DepartmentID { get; set; }

        public List<SelectListItem> Departments { get; set; }

        [Required]
        [DisplayName("Role")]
        public string RoleName { get; set; }

        public List<SelectListItem> Roles {  get; set; }


    }
}
