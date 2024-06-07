namespace MyTeaApp.Models.HelperModels
{
    public class NotificationModel
    {
        public string Type { get; set; }
        public string Message { get; set; }
        public string BgColor { get; set; }
        public NotificationModel(string type, string? message, string? bgColor)
        {
            this.Type = type;

            if(message == null)
            {
                this.Message = type == "success" ? "Success!" : type == "error" ? "Error!" : "Something went bad";
            }
            else
            {
                this.Message = message == "success" ? "Success!" : message == "error" ? "Error!" : message;

            }

            this.BgColor= type == "success" ? "bg-success" : type == "error" ? "bg-danger" : type == "warning" ? "bg-warning" : bgColor;
        }
    }
}
