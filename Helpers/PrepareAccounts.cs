using System.Collections.Generic;

using BackupServiceAPI.Models;

namespace BackupServiceAPI.Helpers
{
    public static class PrepareAccounts
    {
        public static Account RemovePassword(Account account) {
            account.Password = "";
            return account;
        }
        public static List<Account> RemovePasswords(List<Account> accounts) {
            for(int i = 0; i < accounts.Count; i++)
                accounts[i] = RemovePassword(accounts[i]);
            
            return accounts;
        }
    }
}