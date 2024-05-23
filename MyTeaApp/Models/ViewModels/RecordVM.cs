using Microsoft.AspNetCore.Mvc.Rendering;

namespace MyTeaApp.Models.ViewModels
{
    public class RecordVM
    {
        public List<RecordFraction> RecordFractions;

        public int[] SelectedWBS = [0, 0, 0, 0];

        public List<SelectListItem> WBS;

        public WBS WbsCod;
    }

}

