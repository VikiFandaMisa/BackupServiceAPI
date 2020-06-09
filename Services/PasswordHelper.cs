using System.Text;
using System.Security.Cryptography;

namespace BackupServiceAPI.Services {
    public interface IPasswordHelper {
        string CreatePasswordHash(string password);
    }

    public class PasswordHelper : IPasswordHelper {
        public string CreatePasswordHash(string password) {
            var passwordHash = new StringBuilder(512);
            using (var sha = SHA256.Create()) {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                foreach (var b in bytes)
                    passwordHash.AppendFormat("{0:x2}", b);
            }
            return passwordHash.ToString();
        }
    }
}
