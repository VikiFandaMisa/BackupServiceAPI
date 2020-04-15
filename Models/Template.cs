using System;
using System.ComponentModel.DataAnnotations;

namespace BackupServiceAPI.Models {
    public class Template {
        [Key]
        public int ID {get; set;}
        [MaxLength(256)]
        public string Name {get; set;}
        [MaxLength(256)]
        public string Period {get; set;}
        public int Type {get; set;}
        public int TargetFileType {get; set;}
        public DateTime Start {get; set;}
        public DateTime End {get; set;}
        public bool Paused {get; set;}
        public int Retention {get; set;}
    }
}