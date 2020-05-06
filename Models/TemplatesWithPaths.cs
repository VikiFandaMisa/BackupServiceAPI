using System.Collections.Generic;

namespace BackupServiceAPI.Models {
    public class TemplateWithPaths : Template {
        public List<Path> Sources {get; set;}
        public List<Path> Targets {get; set;}
        public static TemplateWithPaths FromTemplate(Template template) {
            return new TemplateWithPaths() {
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
    }
}