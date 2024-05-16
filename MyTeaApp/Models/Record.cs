using System.ComponentModel.DataAnnotations;

namespace MyTeaApp.Models
{
    public class Record
    {
        public int RecordID { get; set; }

        //public ICollection<RecordFraction>

        [Required(ErrorMessage = "{0} Required")]
        [Display(Name = "Total")]
        public float TotalHoursRecord { get; set; }


        //public int? UserID

        //public virtual User user    




    }
}
