using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Text;  
using System.Security.Cryptography;

using BackupServiceAPI.Models;

namespace BackupServiceAPI.Helpers {
    public static class TokenHelper {
        public static async Task<dynamic> GetTokenOwner(ClaimsPrincipal claimsPrincipal, DbBackupServiceContext context) {
            if (claimsPrincipal.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
            {
                string[] identifier = claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value.Split(':');
                string type = identifier[0];
                int id = Convert.ToInt32(identifier[1]);

                if (type == "user") {
                    return await context.Accounts.FindAsync(id);
                }

                if (type == "computer") {
                    return await context.Computers.FindAsync(id);
                }
            }
            return null;
        }

        public static string GetPasswordHash(string password) {
            StringBuilder passwordHash = new StringBuilder(512);
            using (SHA256 sha = SHA256.Create())
            {  
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                foreach (byte b in bytes)
                    passwordHash.AppendFormat("{0:x2}", b);
            }
            return passwordHash.ToString();
        }
    }
}