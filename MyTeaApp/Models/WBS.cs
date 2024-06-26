﻿using System.ComponentModel.DataAnnotations;
using NuGet.Protocol.Plugins;

namespace MyTeaApp.Models
{
    public class WBS
    {
        [Key]
        [Required]
        public int WbsId { get; set; }

        [Required(ErrorMessage = "{0} Required")]
        [Display(Name = "Wbs Title")]
        public string? WbsName { get; set; }

        //[Required(ErrorMessage = "{0} Required")]
        [StringLength(10, MinimumLength = 4, ErrorMessage = "Minimum length for WBS code is 4 characters")]
        [Display(Name = "Código WBS")]
        [RegularExpression(@"^WBS[0-9]{1,7}$", ErrorMessage = "The format must be WBS followed by up to 7 digits.")]
        public string? WbsCod { get; set; }

        [Required(ErrorMessage = "{0} Required")]
        public string? Description { get; set; }


        [Display(Name = "Chargeble")]
        [Required(ErrorMessage = "{0} Required")]
        public bool? IsChargeable { get; set; }

        public ICollection<RecordFraction> RecordFraction { get; set; } = new List<RecordFraction>();


    }
}
