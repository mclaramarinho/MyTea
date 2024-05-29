using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Serialization;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MyTeaApp.Models.ViewModels
{
    public class ValidationResponse
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
    }
    public class RegisterVM
    {
        [Required]
        [DisplayName("Full Name")]
        [DataType(DataType.Text)]
        public string FullName { get; set; }


        [Required]
        [DataType(DataType.EmailAddress, ErrorMessage = "This is not a valid email.")]
        [DisplayName("Email Address")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8), MaxLength(20)]
        [RegularExpression(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$ %^&*-]).{8,}$", 
                            ErrorMessage = "The password must have uppercase and lowercase letters, a number and a special character. It should also be at least 8 characters long.")]
        public string Password { get; set; }

        [Required]
        [DisplayName("Confirm Password")]
        [Compare("Password", ErrorMessage = "The passwords don't match!")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayName("Admission Date")]
        [DisplayFormat(DataFormatString = "{0: dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime AdmissionDate { get; set; }

        [Required(ErrorMessage = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        [DisplayName("Department")]
        
        public int DepartmentID { get; set; }

        public List<SelectListItem> Departments { get; set; }

        [Required]
        [DisplayName("Role")]
        public string RoleName { get; set; }

        public List<SelectListItem> Roles {  get; set; }


        public bool FieldsValid { get; set; } = true;

        public ValidationResponse ValidateDepartmentID()
        {
            ValidationResponse response = new();
            response.IsValid = true;
            
            if(this.DepartmentID == 0)
            {
                response = new ValidationResponse();
                response.IsValid = false;
                response.ErrorMessage = "A department must be selected. If no departments are listed, please, refresh the page.";

                this.FieldsValid = false;
            }

            return response;
        }

        public ValidationResponse ValidateRole()
        {
            ValidationResponse response = new()
            {
                IsValid = this.RoleName != "empty",
                ErrorMessage = this.RoleName == "empty" ? "A role must be selected. If no roles are listed, please, refresh the page." : string.Empty
            };
            this.FieldsValid = !this.FieldsValid ? false : response.IsValid;
            return response;
        }
    }
}
