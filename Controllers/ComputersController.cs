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
            return RemovePasswords(await _context.Computers.ToListAsync());
        }

        // GET: api/Computers/5 
        [HttpGet("{id}")]
        public async Task<ActionResult<Computer>> GetComputer(int id)
        {
            var computer = await _context.Computers.FindAsync(id);

            if (computer == null)
            {
                return NotFound();
            }

            return RemovePassword(computer);
        }

        // PUT: api/Computers/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut]
        public async Task<IActionResult> PutComputer(int id, Computer computer)
        {
            if (computer.Password != "") 
                computer.Password = TokenHelper.GetPasswordHash(computer.Password);

            _context.Entry(computer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ComputerExists(id))
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
            var computer = await _context.Computers.FindAsync(id);
            if (computer == null)
            {
                return NotFound();
            }

            _context.Computers.Remove(computer);
            await _context.SaveChangesAsync();

            return RemovePassword(computer);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<Computer>> RegisterComputer(ComputerRegistration registration)
        {
            Computer toAdd = new Computer() {
                Hostname = registration.Hostname,
                Password = TokenHelper.GetPasswordHash(registration.Password),
                LastSeen = DateTime.Now,
                IP = registration.IP,
                MAC = registration.MAC,
                Status = 1
            };
            _context.Computers.Add(toAdd);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetComputer", new { id = toAdd.ID }, RemovePassword(toAdd));
        }

        private bool ComputerExists(int id)
        {
            return _context.Computers.Any(e => e.ID == id);
        }

        private static Computer RemovePassword(Computer computer) {
            computer.Password = "";
            return computer;
        }

        private static List<Computer> RemovePasswords(List<Computer> computers) {
            for(int i = 0; i < computers.Count; i++)
                computers[i] = RemovePassword(computers[i]);
            
            return computers;
        }
    }
}
