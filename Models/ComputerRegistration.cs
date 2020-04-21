using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackupServiceAPI.Models {
    [NotMapped]
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