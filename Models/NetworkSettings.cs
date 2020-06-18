using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace BackupServiceAPI.Models {
    public enum FTPMode {
        Active = 1,
        Passive = 2
    }

    public enum FTPEncryptionMode {
        None = 1,
        Explicit = 2,
        Implicit = 3
    }

    [NotMapped]
    public class NetworkSettings {
        public string Server { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public FTPMode Mode { get; set; }
        public FTPEncryptionMode Encryption { get; set; }
        public static NetworkSettings FromJson(string s) {
            return JsonSerializer.Deserialize<NetworkSettings>(s);
        }

        public string ToJson() {
            return JsonSerializer.Serialize(this, typeof(NetworkSettings));
        }
    }
}
