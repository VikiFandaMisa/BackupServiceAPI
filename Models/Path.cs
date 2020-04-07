namespace BackupServiceAPI.Models {
    public class Path {
        public int ID {get; set;}
        public int TemplateID {get; set;}
        public string FTP {get; set;}
        public bool Source {get; set;}
        public string Directory {get; set;}
    }
}