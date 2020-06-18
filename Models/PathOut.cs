using System.ComponentModel.DataAnnotations.Schema;

namespace BackupServiceAPI.Models {
    [NotMapped]
    public class PathOut {
        public int ID { get; set; }
        public NetworkSettings Network { get; set; }
        public string Directory { get; set; }

        public static PathOut FromPath(Path path) {
            return new PathOut() {
                ID = path.ID,
                Network = path.Network != null && path.Network != "" ? NetworkSettings.FromJson(path.Network) : null,
                Directory = path.Directory
            };
        }

        public Path ToPath(int templateID, bool source) {
            return new Path() {
                ID = ID,
                TemplateID = templateID,
                Network = Network?.ToJson(),
                Source = source,
                Directory = Directory
            };
        }
    }
}
