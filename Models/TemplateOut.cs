using System.Collections.Generic;
using System;

namespace BackupServiceAPI.Models {
    public class TemplateOut : Template {
        public List<Path> Sources {get; set;}
        public List<Path> Targets {get; set;}

        public static TemplateOut FromTemplate(Template template) {
            return new TemplateOut() {
                ID = template.ID,
                Name = template.Name,
                Period = template.Period,
                Type = template.Type,
                TargetFileType = template.TargetFileType,
                Start = template.Start,
                End = template.End,
                Paused = template.Paused,
                Retention = template.Retention,
                Sources = new List<Path>(),
                Targets = new List<Path>(),
            };
        }
        
        public Template ToTemplate() {
            return new Template() {
                ID = this.ID,
                Name = this.Name,
                Period = this.Period,
                Type = this.Type,
                TargetFileType = this.TargetFileType,
                Start = this.Start,
                End = this.End,
                Paused = this.Paused,
                Retention = this.Retention
            };
        }
    }
}