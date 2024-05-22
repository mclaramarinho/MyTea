using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MyTeaApp.Models.ViewModels
{
    public class UserInfoVM
    {
        public string DbUserId { get; set; }

        public int UserId { get; set; }

        [DisplayName("Name")]
        public string FullName { get; set; }
        
        public string Email { get; set; }

        [DisplayName("Admission Date")]
        [DisplayFormat(DataFormatString = "{0: dd/MM/yyyy}")]
        [DataType(DataType.Date)]
        public DateTime AdmissionDate { get; set; }

        [DisplayName("Department")]
        public string DepartmentName { get; set; }

        [DisplayName("System Role")]
        public string RoleName { get; set; }
        
        public ICollection<Record> Records { get; set; }

        [DisplayName("Currently Employed?")]
        public string IsActive { get; set; }

    }
}
