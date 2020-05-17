using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackupServiceAPI.Models {
    public class PathOut {
        public int ID {get; set;}
        public string FTP {get; set;}
        public string Directory {get; set;}
        public static PathOut FromPath(Path path) {
            return new PathOut() {
                ID = path.ID,
                FTP = path.FTP,
                Directory = path.Directory
            };
        }
        
        public Path ToPath(int templateID, bool source) {
            return new Path() {
                ID = this.ID,
                TemplateID = templateID,
                FTP = this.FTP,
                Source = source,
                Directory = this.Directory
            };
        }
    }
}