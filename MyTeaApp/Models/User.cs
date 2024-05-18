using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTeaApp.Models
{
    public class User : IdentityUser
    {
        public int UserID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0: dd/MM/yyyy}")]
        [DisplayName("Admission Date")]
        public DateTime AdmissionDate { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [DisplayName("Full Name")]
        public string FullName { get; set; }

        [Required]
        [DisplayName("User Active")]
        public bool UserActive { get; set; }

        [ForeignKey("DepartmentId")]
        public int DepartmentId { get; set; }
        public virtual Department Department { get; set; }


        public ICollection<Record> Records { get; set; } = new List<Record>();
                
    }
}
