namespace MyTeaApp.Models.HelperModels.GenerateReport
{
    public class UserInfo
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? DepartmentName { get; set; }
        public bool ActiveUser { get; set; }
        public float TotalWorkedHours { get; set; }
        public List<string> MostUsedWbs { get; set; }
        public List<float> MostUsedWbsHours { get; set; }
        public float NoWorkTime { get; set; }
    }
}
