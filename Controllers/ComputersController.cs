using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using BackupServiceAPI.Models;
using BackupServiceAPI.Services;

namespace BackupServiceAPI.Controllers {
    [Route("api/[controller]")]
    [ApiController, Authorize]
    public class ComputersController : ControllerBase {
        private readonly DbBackupServiceContext _context;
        private readonly ITokenManager _TokenManager;

        public ComputersController(DbBackupServiceContext context, ITokenManager tokenManager) {
            _context = context;
            _TokenManager = tokenManager;
        }

        // GET: api/Computers
        [HttpGet]
        [Authorize(Policy = "UsersOnly")]
        public async Task<ActionResult<IEnumerable<Computer>>> GetComputers() {
            return await _context.Computers.ToListAsync();
        }

        // GET: api/Computers/5 
        [HttpGet("{id}")]
        [Authorize(Policy = "UsersOnly")]
        public async Task<ActionResult<Computer>> GetComputer(int id) {
            var computer = await _context.Computers.FindAsync(id);

            if (computer == null) {
                return NotFound();
            }

            return computer;
        }

        [HttpGet("self")]
        [Authorize(Policy = "ComputersOnly")]
        public async Task<ActionResult<Computer>> GetSelf() {
            var requestor = await _TokenManager.GetTokenOwner();

            Computer computer = await _context.Computers.FindAsync(requestor.ID);

            if (computer == null) {
                return NotFound();
            }

            return computer;
        }

        [HttpPut("self")]
        [Authorize(Policy = "ComputersOnly")]
        public async Task<ActionResult<Computer>> PutSelf(Computer computer) {
            var requestor = await _TokenManager.GetTokenOwner();

            if (requestor.Id != computer.ID)
                return Unauthorized();

            _context.Entry(computer).State = EntityState.Modified;

            try {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) {
                if (!ComputerExists(computer.ID)) {
                    return NotFound();
                }
                else {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<Computer>> RegisterComputer(ComputerRegistration registration) {
            Computer toAdd = new Computer() {
                Hostname = registration.Hostname,
                LastSeen = DateTime.Now,
                IP = registration.IP,
                MAC = registration.MAC,
                Status = ComputerStatus.pending
            };
            _context.Computers.Add(toAdd);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetComputer", new { id = toAdd.ID }, toAdd);
        }

        // PUT: api/Computers/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut]
        [Authorize(Policy = "UsersOnly")]
        public async Task<IActionResult> PutComputer(Computer computer) {
            Computer inDB = await _context.Computers.FindAsync(computer.ID);
            inDB.Status = computer.Status;
            
            _context.Entry(inDB).State = EntityState.Modified;

            try {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) {
                if (!ComputerExists(computer.ID)) {
                    return NotFound();
                }
                else {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Computers/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "UsersOnly")]
        public async Task<ActionResult<Computer>> DeleteComputer(int id) {
            var computer = await _context.Computers.FindAsync(id);
            if (computer == null) {
                return NotFound();
            }

            _context.Computers.Remove(computer);
            await _context.SaveChangesAsync();

            return computer;
        }

        private bool ComputerExists(int id) {
            return _context.Computers.Any(e => e.ID == id);
        }
    }
}
