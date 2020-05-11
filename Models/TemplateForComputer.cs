using System.Collections.Generic;
using System;

namespace BackupServiceAPI.Models {
    public class TemplateForComputer {
        public int ID {get; set;}
        public string Name {get; set;}
        public BackupType Type {get; set;}
        public BackupFileType TargetFileType {get; set;}
        public int Retention {get; set;}
        public DateTime[] Schedule {get; set;}
        public List<Path> Sources {get; set;}
        public List<Path> Targets {get; set;}
        public static TemplateForComputer FromTemplate(Template template) {
            return new TemplateForComputer() {
                ID = template.ID,
                Name = template.Name,
                Type = template.Type,
                TargetFileType = template.TargetFileType,
                Retention = template.Retention,
                Schedule = null,
                Sources = new List<Path>(),
                Targets = new List<Path>(),
            };
        }
    }
}