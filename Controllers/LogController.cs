using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using BackupServiceAPI.Models;

namespace BackupServiceAPI.Controllers {
    [Route("api/[controller]")]
    [ApiController, Authorize]
    public class LogController : ControllerBase {
        private readonly DbBackupServiceContext _context;

        public LogController(DbBackupServiceContext context) {
            _context = context;
        }

        // GET: api/Log
        [HttpGet]
        [Authorize(Policy="UsersOnly")]
        public async Task<ActionResult<IEnumerable<LogItem>>> GetLog() {
            return await _context.Log.ToListAsync();
        }

        // GET: api/Log/5
        [HttpGet("{id}")]
        [Authorize(Policy="UsersOnly")]
        public async Task<ActionResult<LogItem>> GetLogItem(int id) {
            var logItem = await _context.Log.FindAsync(id);

            if (logItem == null) {
                return NotFound();
            }

            return logItem;
        }

        // POST: api/Log
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        [Authorize(Policy="ComputersOnly")]
        public async Task<ActionResult<LogItem>> PostLogItem(LogItem logItem) {
            _context.Log.Add(logItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLogItem", new { id = logItem.ID }, logItem);
        }

        // DELETE: api/Log/5
        [HttpDelete("{id}")]
        [Authorize(Policy="UsersOnly")]
        public async Task<ActionResult<LogItem>> DeleteLogItem(int id) {
            var logItem = await _context.Log.FindAsync(id);
            if (logItem == null) {
                return NotFound();
            }

            _context.Log.Remove(logItem);
            await _context.SaveChangesAsync();

            return logItem;
        }

        private bool LogItemExists(int id) {
            return _context.Log.Any(e => e.ID == id);
        }
    }
}
