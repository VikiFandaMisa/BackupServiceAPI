using System.Text;  
using System.Security.Cryptography;

namespace BackupServiceAPI.Services {
    public interface IPasswordHelper {
        string CreatePasswordHash(string password);
    }
    
    public class PasswordHelper : IPasswordHelper {
        public string CreatePasswordHash(string password) {
            StringBuilder passwordHash = new StringBuilder(512);
            using (SHA256 sha = SHA256.Create()) {  
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                foreach (byte b in bytes)
                    passwordHash.AppendFormat("{0:x2}", b);
            }
            return passwordHash.ToString();
        }
    }
}