using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackupServiceAPI.Models {
    [NotMapped]
    public class TemplateOut {
        public int ID { get; set; }
        public string Name { get; set; }
        public BackupType Type { get; set; }
        public BackupFileType TargetFileType { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool Paused { get; set; }
        public int Retention { get; set; }
        public Period Period { get; set; }
        public List<PathOut> Sources { get; set; }
        public List<PathOut> Targets { get; set; }

        public static TemplateOut FromTemplate(Template template) {
            return new TemplateOut() {
                ID = template.ID,
                Name = template.Name,
                Period = Period.FromJson(template.Period),
                Type = template.Type,
                TargetFileType = template.TargetFileType,
                Start = template.Start,
                End = template.End,
                Paused = template.Paused,
                Retention = template.Retention,
                Sources = new List<PathOut>(),
                Targets = new List<PathOut>(),
            };
        }

        public Template ToTemplate() {
            return new Template() {
                ID = ID,
                Name = Name,
                Period = Period.ToJson(),
                Type = Type,
                TargetFileType = TargetFileType,
                Start = Start,
                End = End,
                Paused = Paused,
                Retention = Retention
            };
        }
    }
}
