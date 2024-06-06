using Microsoft.AspNetCore.Mvc.Rendering;
using MyTeaApp.Models;
using System.Collections.Generic;

namespace MyTeaApp.Views.Records
{
    public class FortnightData
    {

        public List<SelectListItem> Fortnights { get; }

        private DateTime _selectedFortnight { get; set; }
        public DateTime SelectedFortnight { get => _selectedFortnight; }

        private int _firstDayOfFortnight { get; set; }
        public int FirstDayOfFortnight { get => _firstDayOfFortnight; }

        private int _daysInSelectedFortnight { get; set; }
        public int DaysInSelectedFortnight { get => _daysInSelectedFortnight; }

        private DateTime _fortnightStart { get; set; }
        public DateTime FortnightStart { get => _fortnightStart; }

        public FortnightData(string url)
        {
            this.SetSelectedFortnight(url);

            this.SetFirstDayOfFortnight();

            this.Fortnights = new List<SelectListItem>();

            this.CreateSelectList();
        }

        private void SetSelectedFortnight(string url)
        {
            this._selectedFortnight = DateTime.Now;

            if (url != null)
            {
                this._selectedFortnight = DateTime.ParseExact(url, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        private void SetFirstDayOfFortnight()
        {

            this._firstDayOfFortnight = 1;
            if (this.SelectedFortnight.Day > 15)
            {
                this._firstDayOfFortnight = 16;
            }

            this._fortnightStart = new DateTime(this.SelectedFortnight.Year, this.SelectedFortnight.Month, this.FirstDayOfFortnight);
        }

        private void CreateSelectList()
        {
            var currentYear = DateTime.Now.Year;

            for (int i = 1; i <= 12; i++)
            {
                int first = 1;
                int second = 16;

                this.SetFirstFortnight(currentYear, i);

                this.SetSecondFortnight(currentYear, i);
            }
        }

        private void SetFirstFortnight(int currentYear, int index)
        {
            DateTime startDate = new DateTime(currentYear, index, 1);
            DateTime endDate = startDate.AddDays(14);
            bool isSelected = (startDate.Day == this.FortnightStart.Day && startDate.Month == this.FortnightStart.Month && startDate.Year == this.FortnightStart.Year);
            if (isSelected == true)
            {
                this._daysInSelectedFortnight = 15;
            }

            this.Fortnights.Add(new SelectListItem
            {
                Value = $"{startDate:yyyy-MM-dd}",
                Text = $"{startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}",
                Selected = isSelected
            });
        }

        private void SetSecondFortnight(int currentYear, int index)
        {
            int daysInSecondFortnight = 1;

            DateTime tempDate = new DateTime(currentYear, index, 16);

            do
            {
                tempDate = tempDate.AddDays(1);
                if (tempDate.Month == index)
                {
                    daysInSecondFortnight++;
                }
            } while (tempDate.Month == index);

            int daysInMonth = DateTime.DaysInMonth(currentYear, index);

            DateTime startDate = new DateTime(currentYear, index, 16);

            DateTime endDate = new DateTime(currentYear, index, daysInMonth);

            bool isSelected = (startDate.Day == this.FortnightStart.Day && startDate.Month == this.FortnightStart.Month && startDate.Year == this.FortnightStart.Year);

            this._daysInSelectedFortnight = isSelected == true ? daysInSecondFortnight : this.DaysInSelectedFortnight;

            this.Fortnights.Add(new SelectListItem
            {
                Value = $"{startDate:yyyy-MM-dd}",
                Text = $"{startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}",
                Selected = isSelected
            });
        }



    }
}
