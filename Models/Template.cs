using System;
using System.ComponentModel.DataAnnotations;

namespace BackupServiceAPI.Models {
    public enum BackupType {
        Full = 1,
        Differential = 2,
        Incremental = 3
    }

    public enum BackupFileType {
        Plain = 1,
        Zip = 2
    }

    public class Template {
        [Key]
        public int ID { get; set; }
        [MaxLength(256)]
        public string Name { get; set; }
        [MaxLength(256)]
        public string Period { get; set; }
        public BackupType Type { get; set; }
        public BackupFileType TargetFileType { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool Paused { get; set; }
        public int Retention { get; set; }
    }
}
