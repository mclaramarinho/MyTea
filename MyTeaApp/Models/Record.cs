﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTeaApp.Models
{
    public class Record
    {
        public int RecordID { get; set; }

        [Required]
        public List<string> SelectedWbs { get; set; }

        [Required(ErrorMessage = "{0} Required")]
        [Display(Name = "Total")]
        public float TotalHoursRecord { get; set; }

        [ForeignKey("Id")]
        public string? IdUser;
        public virtual User User { get; set; }

        public DateTime StartDate { get; set; }

        public ICollection<RecordFraction> RecordFraction { get; set; } = new List<RecordFraction>();
    }
}
