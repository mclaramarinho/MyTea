using System.ComponentModel.DataAnnotations;
using NuGet.Protocol.Plugins;

namespace MyTeaApp.Models
{
    public class WBS
    {
        [Key]
        [Required]
        public int WbsId { get; set; }

        [Required(ErrorMessage = "{0} Required")]
        [StringLength(10, MinimumLength = 4, ErrorMessage = "Minimum length for WBS code is 4 characters")]
        [Display(Name = "Código WBS")]
        public string? CodWbs { get; set;}

        [Required(ErrorMessage = "{0} Required")]
        public string? Description {  get; set;}

        
        [Display(Name = "Chargeble")]
        [Required(ErrorMessage = "{0} Required")]
        public bool? IsChargeable { get; set; }

        //public ICollection<RecordFraction>


    }
}
