using System;
using System.ComponentModel.DataAnnotations;

namespace BackupServiceAPI.Models {
    public class InvalidatedToken {
        [Key]
        [MaxLength(256)]
        public string Token { get; set; }
        public DateTime Expires { get; set; }
    }
}
