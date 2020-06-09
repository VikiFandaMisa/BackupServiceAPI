using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackupServiceAPI.Models {
    public class Path {
        [Key]
        public int ID { get; set; }
        [ForeignKey("TemplatesID")]
        public int TemplateID { get; set; }
        [MaxLength(256)]
        public string Network { get; set; }
        public bool Source { get; set; }
        [MaxLength(256)]
        public string Directory { get; set; }
    }
}
