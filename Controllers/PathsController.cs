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
    public class PathsController : ControllerBase
    {
        private readonly DbBackupServiceContext _context;

        public PathsController(DbBackupServiceContext context)
        {
            _context = context;
        }

        // GET: api/Paths
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Path>>> GetPaths()
        {
            return await _context.Paths.ToListAsync();
        }

        // GET: api/Paths/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Path>> GetPath(int id)
        {
            var path = await _context.Paths.FindAsync(id);

            if (path == null)
            {
                return NotFound();
            }

            return path;
        }

        // PUT: api/Paths/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPath(int id, Path path)
        {
            if (id != path.ID)
            {
                return BadRequest();
            }

            _context.Entry(path).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PathExists(id))
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

        // POST: api/Paths
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Path>> PostPath(Path path)
        {
            _context.Paths.Add(path);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPath", new { id = path.ID }, path);
        }

        // DELETE: api/Paths/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Path>> DeletePath(int id)
        {
            var path = await _context.Paths.FindAsync(id);
            if (path == null)
            {
                return NotFound();
            }

            _context.Paths.Remove(path);
            await _context.SaveChangesAsync();

            return path;
        }

        private bool PathExists(int id)
        {
            return _context.Paths.Any(e => e.ID == id);
        }
    }
}
