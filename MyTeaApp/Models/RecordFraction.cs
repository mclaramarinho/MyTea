using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyTeaApp.Models;

namespace MyTeaApp.Models
{
    public class RecordFraction
    { 

        [Key]
        public Guid RecordFractionID { get; set; }

        [ForeignKey("WbsID")]
        [Display(Name = "WBS")]
        public int WbsID { get; set; }
        public virtual WBS Wbs { get; set; }

        [Required(ErrorMessage = "{0} Required")]
        [Display(Name = "Record Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        [DataType(DataType.Date)]
        public DateTime RecordDate { get; set; }


        [Display(Name = "Total Hours")]
        [Required(ErrorMessage = "{0} Required")]
        public float TotalHoursFraction { get; set; }


        [ForeignKey("RecordID")]
        public int RecordID { get; set; }
        public virtual Record Record { get; set; }
    }
}
