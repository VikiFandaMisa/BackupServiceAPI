using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackupServiceAPI.Models {
    [NotMapped]
    public class LoginComputer
    {
        [Required]
        public int ID { get; set; }
    }
}