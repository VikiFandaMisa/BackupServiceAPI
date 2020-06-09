using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackupServiceAPI.Models {
    [NotMapped]
    public class JobOut {
        public int TemplateID { get; set; }
        public string TemplateName { get; set; }
        public int ID { get; set; }
        public BackupType Type { get; set; }
        public BackupFileType TargetFileType { get; set; }
        public int Retention { get; set; }
        public List<DateTime> Schedule { get; set; }
        public List<Path> Sources { get; set; }
        public List<Path> Targets { get; set; }

        public static JobOut FromTemplate(Template template, int id, List<DateTime> schedule) {
            return new JobOut() {
                TemplateID = template.ID,
                TemplateName = template.Name,
                ID = id,
                Type = template.Type,
                TargetFileType = template.TargetFileType,
                Retention = template.Retention,
                Schedule = schedule,
                Sources = new List<Path>(),
                Targets = new List<Path>(),
            };
        }
    }
}
