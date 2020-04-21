using System;
using System.ComponentModel.DataAnnotations;

namespace BackupServiceAPI.Models {
    public class ComputerRegistration {
        [MaxLength(256)]
        public string Hostname {get; set;}
        [MaxLength(256)]
        public string Password {get; set;}
        public string IP {get; set;}
        [MaxLength(256)]
        public string MAC {get; set;}
    }
}