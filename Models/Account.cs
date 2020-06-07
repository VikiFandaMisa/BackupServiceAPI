using System.ComponentModel.DataAnnotations;

namespace BackupServiceAPI.Models {
    public class Account {
        [Key]
        public int ID {get; set;}
        [MaxLength(256)]
        public string Username {get; set;}
        [MaxLength(256)]
        public string Password {get; set;}
        public bool Admin {get; set;}
        [MaxLength(256)]
        public string Email {get; set;}
        public bool SendReports {get; set;}
    }
}