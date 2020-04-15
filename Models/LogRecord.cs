using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackupServiceAPI.Models {
    public class LogRecord {
        [Key]
        public int ID {get; set;}
        [ForeignKey("JobsID")]
        public int JobID {get; set;}
        public int Type {get; set;}
        public DateTime Date {get; set;}
        [MaxLength(256)]
        public string Message {get; set;}
    }
}