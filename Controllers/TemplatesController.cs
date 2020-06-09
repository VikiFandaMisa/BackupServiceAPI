using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BackupServiceAPI.Models;

namespace BackupServiceAPI.Controllers {
    [Route("api/[controller]")]
    [ApiController, Authorize(Policy = "UsersOnly")]
    public class TemplatesController : ControllerBase {
        private readonly DbBackupServiceContext _Context;

        public TemplatesController(DbBackupServiceContext context) {
            _Context = context;
        }

        // GET: api/Templates
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TemplateOut>>> GetTemplates() {
            var templates = await _Context.Templates.ToListAsync();

            var templateOut = new TemplateOut[templates.Count];
            for (var i = 0; i < templates.Count; i++)
                templateOut[i] = TemplateToTemplateOut(templates[i]);

            return templateOut;
        }

        // GET: api/Templates/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TemplateOut>> GetTemplate(int id) {
            var template = await _Context.Templates.FindAsync(id);

            if (template == null) {
                return NotFound();
            }

            return TemplateToTemplateOut(template);
        }

        // PUT: api/Templates/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut]
        public async Task<IActionResult> PutTemplate(TemplateOut templateOut) {
            var unpacked = TemplateOutToTemplateAndPaths(templateOut);

            _Context.Entry(unpacked.Item1).State = EntityState.Modified;

            var storedPaths = _Context.Paths.FromSqlRaw(@"
                SELECT *
                FROM Paths p
                WHERE TemplateID = " + unpacked.Item1.ID
            ).ToList();

            foreach (var p in unpacked.Item2) {
                var stored = false;
                for (var i = 0; i < storedPaths.Count; i++)
                    if (storedPaths[i].ID == p.ID) {
                        stored = true;
                        storedPaths.RemoveAt(i);
                        _Context.Entry(p).State = EntityState.Modified;
                        break;
                    }
                if (!stored) {
                    _Context.Paths.Add(p);
                }
            }

            foreach (var deleted in storedPaths) {
                _Context.Paths.Remove(deleted);
            }

            try {
                await _Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) {
                if (!TemplateExists(unpacked.Item1.ID)) {
                    return NotFound();
                }
                else {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Templates
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<TemplateOut>> PostTemplate(TemplateOut templateOut) {
            var unpacked = TemplateOutToTemplateAndPaths(templateOut);

            _Context.Templates.Add(unpacked.Item1);

            await _Context.SaveChangesAsync();

            var templateRet = TemplateToTemplateOut(unpacked.Item1);

            foreach (var p in unpacked.Item2) {
                p.TemplateID = templateRet.ID;
                _Context.Paths.Add(p);
            }

            await _Context.SaveChangesAsync();

            return CreatedAtAction("GetTemplate", new { id = templateRet.ID }, templateRet);
        }

        // DELETE: api/Templates/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<TemplateOut>> DeleteTemplate(int id) {
            var template = await _Context.Templates.FindAsync(id);
            if (template == null) {
                return NotFound();
            }

            var templateOut = TemplateToTemplateOut(template);

            foreach (var p in GetPaths(template.ID))
                _Context.Paths.Remove(p);

            var jobs = _Context.Jobs.FromSqlRaw(@"
                SELECT *
                FROM Jobs j
                WHERE TemplateID = " + id
            );

            await jobs.ForEachAsync(job => {
                var logs = _Context.Log.FromSqlRaw(@"
                    SELECT *
                    FROM Logs l
                    WHERE JobID = " + job.ID
                );
                _Context.Log.RemoveRange(logs);
            });

            _Context.Jobs.RemoveRange(jobs);
            await _Context.SaveChangesAsync();

            _Context.Templates.Remove(template);
            await _Context.SaveChangesAsync();

            return templateOut;
        }

        private bool TemplateExists(int id) {
            return _Context.Templates.Any(e => e.ID == id);
        }

        private TemplateOut TemplateToTemplateOut(Template template) {
            var tOut = TemplateOut.FromTemplate(template);

            foreach (var path in GetPaths(template.ID)) {
                if (path.Source)
                    tOut.Sources.Add(PathOut.FromPath(path));
                else
                    tOut.Targets.Add(PathOut.FromPath(path));
            }

            return tOut;
        }

        private Path[] GetPaths(int templateID) {
            return _Context.Paths.FromSqlRaw(@"
                SELECT *
                FROM Paths p
                WHERE TemplateID = " + templateID
            ).ToArray();
        }

        private (Template, List<Path>) TemplateOutToTemplateAndPaths(TemplateOut templateOut) {
            var template = templateOut.ToTemplate();
            var paths = new List<Path>();

            foreach (var p in templateOut.Sources)
                paths.Add(p.ToPath(template.ID, true));

            foreach (var p in templateOut.Targets)
                paths.Add(p.ToPath(template.ID, false));

            return (template, paths);
        }
    }
}
