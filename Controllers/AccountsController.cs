using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

using BackupServiceAPI.Models;
using BackupServiceAPI.Services;

namespace BackupServiceAPI.Controllers {
    [Route("api/[controller]")]
    [ApiController, Authorize(Policy = "UsersOnly")]
    public class AccountsController : ControllerBase {
        private readonly DbBackupServiceContext _Context;
        private readonly ITokenManager _TokenManager;
        private readonly IPasswordHelper _PasswordHelper;

        public AccountsController(DbBackupServiceContext context, ITokenManager tokenManager, IPasswordHelper passwordHelper) {
            _Context = context;
            _TokenManager = tokenManager;
            _PasswordHelper = passwordHelper;
        }

        // GET: api/Accounts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Account>>> GetAccounts() {
            var requestor = await _TokenManager.GetTokenOwner();

            if (!requestor.Admin)
                return Unauthorized();

            return RemovePasswords(await _Context.Accounts.ToListAsync());

        }

        [HttpGet("self")]
        public async Task<ActionResult<Account>> GetSelf() {
            var requestor = await _TokenManager.GetTokenOwner();

            Account account = await _Context.Accounts.FindAsync(requestor.ID);

            if (account == null) {
                return NotFound();
            }

            return RemovePassword(account);
        }

        // GET: api/Accounts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Account>> GetAccount(int id) {
            var requestor = await _TokenManager.GetTokenOwner();

            if (!(requestor.Admin || id == requestor.ID))
                return Unauthorized();

            var account = await _Context.Accounts.FindAsync(id);

            if (account == null) {
                return NotFound();
            }

            return RemovePassword(account);
        }

        // PUT: api/Accounts/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut]
        public async Task<IActionResult> PutAccount(Account account) {
            var requestor = await _TokenManager.GetTokenOwner();

            if (!(requestor.Admin || account.ID == requestor.ID))
                return Unauthorized();

            if (account.Password != "" && account.ID == requestor.ID)
                account.Password = _PasswordHelper.CreatePasswordHash(account.Password);
            else
                await ReturnPassword(account);

            _Context.Entry(account).State = EntityState.Modified;

            try {
                await _Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) {
                if (!AccountExists(account.ID)) {
                    return NotFound();
                }
                else {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Accounts
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Account>> PostAccount(Account account) {
            var requestor = await _TokenManager.GetTokenOwner();

            if (!requestor.Admin)
                return Unauthorized();

            _Context.Accounts.Add(account);
            await _Context.SaveChangesAsync();

            return CreatedAtAction("GetAccount", new { id = account.ID }, RemovePassword(account));
        }

        // DELETE: api/Accounts/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Account>> DeleteAccount(int id) {
            var requestor = await _TokenManager.GetTokenOwner();

            if (!requestor.Admin)
                return Unauthorized();

            var account = await _Context.Accounts.FindAsync(id);
            if (account == null) {
                return NotFound();
            }

            _Context.Accounts.Remove(account);
            await _Context.SaveChangesAsync();

            return RemovePassword(account);
        }

        private bool AccountExists(int id) {
            return _Context.Accounts.Any(e => e.ID == id);
        }

        private static Account RemovePassword(Account account) {
            account.Password = "";
            return account;
        }
        private static List<Account> RemovePasswords(List<Account> accounts) {
            for (var i = 0; i < accounts.Count; i++)
                accounts[i] = RemovePassword(accounts[i]);

            return accounts;
        }
        private async Task<Account> ReturnPassword(Account account) {
            account.Password = (
                await _Context.Accounts.AsNoTracking().SingleOrDefaultAsync(item => item.ID == account.ID)
            ).Password;
            return account;
        }
    }
}
