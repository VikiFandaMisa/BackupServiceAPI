using System;

namespace BackupServiceAPI.Models {
    public class Template {
        public int ID {get; set;}
        public string Name {get; set;}
        public string Period {get; set;}
        public int Type {get; set;}
        public int TargetFileType {get; set;}
        public DateTime Start {get; set;}
        public DateTime End {get; set;}
        public bool Paused {get; set;}
        public int Retention {get; set;}
    }
}