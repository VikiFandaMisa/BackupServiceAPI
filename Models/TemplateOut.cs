using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackupServiceAPI.Models {
    [NotMapped]
    public class TemplateOut : Template {
        public List<PathOut> Sources {get; set;}
        public List<PathOut> Targets {get; set;}

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
                Sources = new List<PathOut>(),
                Targets = new List<PathOut>(),
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