using System.ComponentModel.DataAnnotations;

namespace MyTeaApp.Models.ViewModels
{
    public class ChangePasswordVM
    {
        [Required(ErrorMessage = "Old password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Old Password")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
