using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

using BackupServiceAPI.Models;

namespace BackupServiceAPI.Helpers {
    public static class Tokens {
        public static async Task<Account> GetTokenUser(ClaimsPrincipal claimsPrincipal, DbBackupServiceContext context) {
            if (claimsPrincipal.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
            {
                int id = Convert.ToInt32(
                    claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value
                );

                return await context.Accounts.FindAsync(id);
            }
            return null;
        }
    }
}