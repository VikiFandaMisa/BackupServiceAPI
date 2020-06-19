using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NCrontab;

using BackupServiceAPI.Models;
using BackupServiceAPI.Services;

namespace BackupServiceAPI.Controllers {
    [Route("api/[controller]")]
    [ApiController, Authorize]
    public class JobsController : ControllerBase {
        private readonly DbBackupServiceContext _Context;
        private readonly IConfiguration _Configuration;
        private readonly ITokenManager _TokenManager;

        public JobsController(DbBackupServiceContext context, IConfiguration configuration, ITokenManager tokenManager) {
            _Context = context;
            _Configuration = configuration;
            _TokenManager = tokenManager;
        }

        // GET: api/Jobs
        [HttpGet]
        [Authorize(Policy = "UsersOnly")]
        public async Task<ActionResult<IEnumerable<Job>>> GetJobs() {
            return await _Context.Jobs.ToListAsync();
        }

        // GET: api/Jobs/5
        [HttpGet("{id}")]
        [Authorize(Policy = "UsersOnly")]
        public async Task<ActionResult<Job>> GetJob(int id) {
            var job = await _Context.Jobs.FindAsync(id);

            if (job == null) {
                return NotFound();
            }

            return job;
        }

        [HttpGet("computer")]
        [Authorize(Policy = "ComputersOnly")]
        public async Task<ActionResult<JobOut[]>> GetComputerJobs() {
            Computer requestor = await _TokenManager.GetTokenOwner();

            requestor.LastSeen = DateTime.Now;
            _Context.Entry(requestor).State = EntityState.Modified;

            await _Context.SaveChangesAsync();

            var templates = _Context.Templates.FromSqlRaw(@"
                SELECT t.*
                FROM Templates t
                    INNER JOIN Jobs j ON t.ID = j.TemplateID
                WHERE t.Paused != true AND j.Active = true AND ComputerID = " + requestor.ID
            ).ToArray();

            var jobsOut = new JobOut[templates.Length];
            for (var i = 0; i < templates.Length; i++) {
                var schedule = GetSchedule(Period.FromJson(templates[i].Period).GetCron(), templates[i].Start, templates[i].End);
                var templateReturn = JobOut.FromTemplate(templates[i], 0, schedule); //LOL FIX THIS LATER
                jobsOut[i] = templateReturn;

                var paths = _Context.Paths.FromSqlRaw(@"
                    SELECT *
                    FROM Paths p
                    WHERE TemplateID = " + templates[i].ID
                ).ToArray();

                foreach (var path in paths) {
                    if (path.Source)
                        jobsOut[i].Sources.Add(path);
                    else
                        jobsOut[i].Targets.Add(path);
                }
            }

            return jobsOut;
        }

        // PUT: api/Jobs/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut]
        [Authorize(Policy = "UsersOnly")]
        public async Task<IActionResult> PutJob(Job job) {
            _Context.Entry(job).State = EntityState.Modified;

            try {
                await _Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) {
                if (!JobExists(job.ID)) {
                    return NotFound();
                }
                else {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Jobs
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        [Authorize(Policy = "UsersOnly")]
        public async Task<ActionResult<Job>> PostJob(Job job) {
            _Context.Jobs.Add(job);
            await _Context.SaveChangesAsync();

            return CreatedAtAction("GetJob", new { id = job.ID }, job);
        }

        // DELETE: api/Jobs/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "UsersOnly")]
        public async Task<ActionResult<Job>> DeleteJob(int id) {
            var job = await _Context.Jobs.FindAsync(id);
            if (job == null) {
                return NotFound();
            }

            var logs = _Context.Log.FromSqlRaw(@"
                SELECT *
                FROM Log l
                WHERE JobID = " + id
            );

            _Context.Log.RemoveRange(logs);
            await _Context.SaveChangesAsync();

            _Context.Jobs.Remove(job);
            await _Context.SaveChangesAsync();

            return job;
        }

        private bool JobExists(int id) {
            return _Context.Jobs.Any(e => e.ID == id);
        }

        private List<DateTime> GetSchedule(string cron, DateTime start, DateTime end) {
            System.Console.WriteLine(cron);
            var schedule = new List<DateTime>();
            var scheduleLength = Convert.ToInt32(_Configuration["Jobs:ScheduleLength"]);
            var crontab = CrontabSchedule.Parse(cron);

            for (var i = 0; i < scheduleLength; i++) {
                var add = crontab.GetNextOccurrence(start);
                if (add < end)
                    schedule.Add(add);
                else
                    break;
                start = add;
            }

            return schedule;
        }
    }
}
