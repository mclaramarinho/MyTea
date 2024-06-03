using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace MyTeaApp.Models.ViewModels
{
    public class RecordVM
    {
        public Record? ExistingRecord { get; set; }
        public List<SelectListItem> WBS;

        public User user { get; set; }
    }

}

