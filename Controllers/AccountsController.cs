using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

using BackupServiceAPI.Helpers;
using BackupServiceAPI.Models;

namespace BackupServiceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController, Authorize(Policy = "UsersOnly")]
    public class AccountsController : ControllerBase
    {
        private readonly DbBackupServiceContext _context;

        public AccountsController(DbBackupServiceContext context)
        {
            _context = context;
        }

        // GET: api/Accounts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Account>>> GetAccounts()
        {
            var requestor = await TokenHelper.GetTokenOwner(HttpContext.User, _context);

            if (!requestor.Admin)
                return Unauthorized();
            
            return RemovePasswords(await _context.Accounts.ToListAsync());

        }

        [HttpGet("self")]
        public async Task<ActionResult<Account>> GetSelf()
        {
            var requestor = await TokenHelper.GetTokenOwner(HttpContext.User, _context);

            Account account = await _context.Accounts.FindAsync(requestor.ID);

            if (account == null)
            {
                return NotFound();
            }

            return RemovePassword(account);
        }

        // GET: api/Accounts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Account>> GetAccount(int id)
        {
            var requestor = await TokenHelper.GetTokenOwner(HttpContext.User, _context);

            if (!(requestor.Admin || id == requestor.ID))
                return Unauthorized();

            Account account = await _context.Accounts.FindAsync(id);

            if (account == null)
            {
                return NotFound();
            }

            return RemovePassword(account);
        }

        // PUT: api/Accounts/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut]
        public async Task<IActionResult> PutAccount(Account account)
        {
            var requestor = await TokenHelper.GetTokenOwner(HttpContext.User, _context);

            if (!(requestor.Admin || account.ID == requestor.ID))
                return Unauthorized();

            if (account.Password != "")
                account.Password = TokenHelper.CreatePasswordHash(account.Password);
            else
                await ReturnPassword(account);

            _context.Entry(account).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AccountExists(account.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Accounts
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Account>> PostAccount(Account account)
        {
            var requestor = await TokenHelper.GetTokenOwner(HttpContext.User, _context);

            if (!requestor.Admin)
                return Unauthorized();

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAccount", new { id = account.ID }, RemovePassword(account));
        }

        // DELETE: api/Accounts/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Account>> DeleteAccount(int id)
        {
            var requestor = await TokenHelper.GetTokenOwner(HttpContext.User, _context);

            if (!requestor.Admin)
                return Unauthorized();

            Account account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();

            return RemovePassword(account);
        }

        private bool AccountExists(int id)
        {
            return _context.Accounts.Any(e => e.ID == id);
        }

        private static Account RemovePassword(Account account) {
            account.Password = "";
            return account;
        }
        private static List<Account> RemovePasswords(List<Account> accounts) {
            for(int i = 0; i < accounts.Count; i++)
                accounts[i] = RemovePassword(accounts[i]);
            
            return accounts;
        }
        private async Task<Account> ReturnPassword(Account account) {
            account.Password = (
                await _context.Accounts.FindAsync(account.ID)
            ).Password;
            return account;
        }
    }
}
