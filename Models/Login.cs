using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackupServiceAPI.Models {
    [NotMapped]
    public class Login
    {
        [Required, MaxLength(256)]
        public string Username { get; set; }
        [Required, MaxLength(256)]
        public string Password { get; set; }
    }
}