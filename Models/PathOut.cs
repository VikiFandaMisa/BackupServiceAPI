using System.ComponentModel.DataAnnotations.Schema;

namespace BackupServiceAPI.Models {
    [NotMapped]
    public class PathOut {
        public int ID {get; set;}
        public string Network {get; set;}
        public string Directory {get; set;}
        
        public static PathOut FromPath(Path path) {
            return new PathOut() {
                ID = path.ID,
                Network = path.Network,
                Directory = path.Directory
            };
        }
        
        public Path ToPath(int templateID, bool source) {
            return new Path() {
                ID = this.ID,
                TemplateID = templateID,
                Network = this.Network,
                Source = source,
                Directory = this.Directory
            };
        }
    }
}