using System.ComponentModel.DataAnnotations;

namespace MyTeaApp.Models
{
    public class Department
    {
        [Key]
        [Required]
        [Display(Name = "Department ID")]
        public int DepartmentID { get; set; }

        [Display(Name = "Department Name")]
        [Required(ErrorMessage = "{0} Required")]
        public  string? DepartmentName { get; set; }

        // public ICollection<User>
    }
}
