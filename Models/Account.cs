namespace BackupServiceAPI.Models {
    public class Account {
        public int ID {get; set;}
        public string Username {get; set;}
        public string Password {get; set;}
        public bool Admin {get; set;}
        public string Email {get; set;}
        public bool SendReports {get; set;}
        public string ReportPeriod {get; set;}
    }
}