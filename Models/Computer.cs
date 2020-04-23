using System;
using System.ComponentModel.DataAnnotations;

namespace BackupServiceAPI.Models {
    public enum ComputerStatus {
        pending = 1,
        approved = 2,
        denied = 3
    }
    public class Computer {
        [Key]
        public int ID { get; set;}
        [MaxLength(256)]
        public string Hostname {get; set;}
        public DateTime LastSeen {get; set;}
        [MaxLength(256)]
        public string IP {get; set;}
        [MaxLength(256)]
        public string MAC {get; set;}
        public ComputerStatus Status {get; set;}
    }
}