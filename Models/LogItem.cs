using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackupServiceAPI.Models {
    public enum MessageType {
        error = 1,
        info = 2,
        job = 3
    }
    public class LogItem {
        [Key]
        public int ID {get; set;}
        [ForeignKey("JobsID")]
        public int JobID {get; set;}
        public MessageType Type {get; set;}
        public DateTime Date {get; set;}
        [MaxLength(256)]
        public string Message {get; set;}
    }
}