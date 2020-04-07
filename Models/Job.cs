namespace BackupServiceAPI.Models {
    public class Job {
        public int ID {get; set;}
        public int ComputerID {get; set;}
        public int TemplateID {get; set;}
        public bool Active {get; set;}
    }
}