using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BackupServiceAPI.Models;

namespace BackupServiceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController, Authorize]
    public class LogController : ControllerBase
    {
        private readonly DbBackupServiceContext _context;

        public LogController(DbBackupServiceContext context)
        {
            _context = context;
        }

        // GET: api/Log
        [HttpGet]
        [Authorize(Policy="UsersOnly")]
        public async Task<ActionResult<IEnumerable<LogRecord>>> GetLog()
        {
            return await _context.Log.ToListAsync();
        }

        // GET: api/Log/5
        [HttpGet("{id}")]
        [Authorize(Policy="UsersOnly")]
        public async Task<ActionResult<LogRecord>> GetLogRecord(int id)
        {
            var logRecord = await _context.Log.FindAsync(id);

            if (logRecord == null)
            {
                return NotFound();
            }

            return logRecord;
        }

        // PUT: api/Log/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        [Authorize(Policy="UsersOnly")]
        public async Task<IActionResult> PutLogRecord(int id, LogRecord logRecord)
        {
            if (id != logRecord.ID)
            {
                return BadRequest();
            }

            _context.Entry(logRecord).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LogRecordExists(id))
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

        // POST: api/Log
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        [Authorize(Policy="UsersOnly")]
        public async Task<ActionResult<LogRecord>> PostLogRecord(LogRecord logRecord)
        {
            _context.Log.Add(logRecord);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLogRecord", new { id = logRecord.ID }, logRecord);
        }

        // DELETE: api/Log/5
        [HttpDelete("{id}")]
        [Authorize(Policy="UsersOnly")]
        public async Task<ActionResult<LogRecord>> DeleteLogRecord(int id)
        {
            var logRecord = await _context.Log.FindAsync(id);
            if (logRecord == null)
            {
                return NotFound();
            }

            _context.Log.Remove(logRecord);
            await _context.SaveChangesAsync();

            return logRecord;
        }

        private bool LogRecordExists(int id)
        {
            return _context.Log.Any(e => e.ID == id);
        }
    }
}
