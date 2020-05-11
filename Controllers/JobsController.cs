using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

using BackupServiceAPI.Models;
using BackupServiceAPI.Helpers;

namespace BackupServiceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController, Authorize]
    public class JobsController : ControllerBase
    {
        private readonly DbBackupServiceContext _context;

        public JobsController(DbBackupServiceContext context)
        {
            _context = context;
        }

        // GET: api/Jobs
        [HttpGet]
        [Authorize(Policy="UsersOnly")]
        public async Task<ActionResult<IEnumerable<Job>>> GetJobs()
        {
            return await _context.Jobs.ToListAsync();
        }

        // GET: api/Jobs/5
        [HttpGet("{id}")]
        [Authorize(Policy="UsersOnly")]
        public async Task<ActionResult<Job>> GetJob(int id)
        {
            var job = await _context.Jobs.FindAsync(id);

            if (job == null)
            {
                return NotFound();
            }

            return job;
        }

        [HttpGet("computer")]
        [Authorize(Policy="ComputersOnly")]
        public async Task<ActionResult<TemplateForComputer[]>> GetComputerJobs()
        {
            Computer requestor = await TokenHelper.GetTokenOwner(HttpContext.User, _context);

            Template[] templates = _context.Templates.FromSqlRaw(@"
                SELECT
                    t.ID,
                    t.Name,
                    t.Period,
                    t.Type,
                    t.TargetFileType,
                    t.Start,
                    t.End,
                    t.Paused,
                    t.Retention
                FROM Templates t
                    INNER JOIN Jobs j ON t.ID = j.TemplateID
                WHERE t.Paused != true AND ComputerID = " + requestor.ID
            ).ToArray();

            TemplateForComputer[] templatesForComputer = new TemplateForComputer[templates.Length];
            for (int i = 0; i < templates.Length; i++)
            {
                TemplateForComputer templateReturn = TemplateForComputer.FromTemplate(templates[i]);
                templatesForComputer[i] = templateReturn;

                templateReturn.Schedule = TemplatesHelper.GetSchedule(templates[i].Period);

                Path[] paths = _context.Paths.FromSqlRaw(@"
                    SELECT *
                    FROM Paths p
                    WHERE TemplateID = " + templates[i].ID
                ).ToArray();

                foreach (Path path in paths)
                {
                    if (path.Source)
                        templatesForComputer[i].Sources.Add(path);
                    else 
                        templatesForComputer[i].Targets.Add(path);
                }
            }

            return templatesForComputer;
        }

        // PUT: api/Jobs/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        [Authorize(Policy="UsersOnly")]
        public async Task<IActionResult> PutJob(int id, Job job)
        {
            if (id != job.ID)
            {
                return BadRequest();
            }

            _context.Entry(job).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JobExists(id))
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

        // POST: api/Jobs
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        [Authorize(Policy="UsersOnly")]
        public async Task<ActionResult<Job>> PostJob(Job job)
        {
            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetJob", new { id = job.ID }, job);
        }

        // DELETE: api/Jobs/5
        [HttpDelete("{id}")]
        [Authorize(Policy="UsersOnly")]
        public async Task<ActionResult<Job>> DeleteJob(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null)
            {
                return NotFound();
            }

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();

            return job;
        }

        private bool JobExists(int id)
        {
            return _context.Jobs.Any(e => e.ID == id);
        }
    }
}
