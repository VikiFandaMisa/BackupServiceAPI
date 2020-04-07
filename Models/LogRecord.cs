using System;

namespace BackupServiceAPI.Models {
    public class LogRecord {
        public int ID {get; set;}
        public int JobID {get; set;}
        public int Type {get; set;}
        public DateTime Date {get; set;}
        public string Message {get; set;}
    }
}