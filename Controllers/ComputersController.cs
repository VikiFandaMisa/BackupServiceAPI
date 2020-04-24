using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackupServiceAPI.Models;

using BackupServiceAPI.Helpers;

namespace BackupServiceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController, Authorize]
    public class ComputersController : ControllerBase
    {
        private readonly DbBackupServiceContext _context;

        public ComputersController(DbBackupServiceContext context)
        {
            _context = context;
        }

        // GET: api/Computers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Computer>>> GetComputers()
        {
            var requestor = await TokenHelper.GetTokenOwner(HttpContext.User, _context);

            if (requestor is Computer)
                return Unauthorized();

            return await _context.Computers.ToListAsync();
        }

        // GET: api/Computers/5 
        [HttpGet("{id}")]
        public async Task<ActionResult<Computer>> GetComputer(int id)
        {
            var requestor = await TokenHelper.GetTokenOwner(HttpContext.User, _context);

            if (!(requestor is Computer && requestor.ID == id) && !(requestor is Account))
                return Unauthorized();

            var computer = await _context.Computers.FindAsync(id);

            if (computer == null)
            {
                return NotFound();
            }

            return computer;
        }

        [HttpGet("self")]
        public async Task<ActionResult<Computer>> GetSelf()
        {
            var requestor = await TokenHelper.GetTokenOwner(HttpContext.User, _context);

            if (requestor is Account)
                return Unauthorized();

            Computer computer = await _context.Computers.FindAsync(requestor.ID);

            if (computer == null)
            {
                return NotFound();
            }

            return computer;
        }

        // PUT: api/Computers/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut]
        public async Task<IActionResult> PutComputer(Computer computer)
        {
            var requestor = await TokenHelper.GetTokenOwner(HttpContext.User, _context);

            if (!(requestor is Computer && requestor.ID == computer.ID) && !(requestor is Account))
                return Unauthorized();

            _context.Entry(computer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ComputerExists(computer.ID))
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

        // DELETE: api/Computers/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Computer>> DeleteComputer(int id)
        {
            var requestor = await TokenHelper.GetTokenOwner(HttpContext.User, _context);

            if (requestor is Computer)
                return Unauthorized();

            var computer = await _context.Computers.FindAsync(id);
            if (computer == null)
            {
                return NotFound();
            }

            _context.Computers.Remove(computer);
            await _context.SaveChangesAsync();

            return computer;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<Computer>> RegisterComputer(ComputerRegistration registration)
        {
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

        private bool ComputerExists(int id)
        {
            return _context.Computers.Any(e => e.ID == id);
        }
    }
}
