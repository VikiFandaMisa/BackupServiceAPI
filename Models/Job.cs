using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackupServiceAPI.Models {
    public class Job {
        [Key]
        public int ID { get; set; }
        [ForeignKey("ComputersID")]
        public int ComputerID { get; set; }
        [ForeignKey("TemplatesID")]
        public int TemplateID { get; set; }
        public bool Active { get; set; }
    }
}
