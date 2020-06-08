using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BackupServiceAPI.Models;

namespace BackupServiceAPI.Controllers {
    [Route("api/[controller]")]
    [ApiController, Authorize(Policy="UsersOnly")]
    public class TemplatesController : ControllerBase {
        private readonly DbBackupServiceContext _context;

        public TemplatesController(DbBackupServiceContext context) {
            _context = context;
        }

        // GET: api/Templates
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TemplateOut>>> GetTemplates() {
            List<Template> templates = await _context.Templates.ToListAsync();

            TemplateOut[] templateOut = new TemplateOut[templates.Count];
            for (int i = 0; i < templates.Count; i++)
                templateOut[i] = TemplateToTemplateOut(templates[i]);

            return templateOut;
        }

        // GET: api/Templates/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TemplateOut>> GetTemplate(int id) {
            var template = await _context.Templates.FindAsync(id);

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

            _context.Entry(unpacked.Item1).State = EntityState.Modified;

            var storedPaths = _context.Paths.FromSqlRaw(@"
                SELECT *
                FROM Paths p
                WHERE TemplateID = " + unpacked.Item1.ID
            ).ToList();

            foreach (Path p in unpacked.Item2) {
                var stored = false;
                for (int i = 0; i < storedPaths.Count; i++)
                    if (storedPaths[i].ID == p.ID) {
                        stored = true;
                        storedPaths.RemoveAt(i);
                        _context.Entry(p).State = EntityState.Modified;
                        break;
                    }
                if (!stored) {
                    _context.Paths.Add(p);
                }
            }

            foreach (Path deleted in storedPaths) {
                _context.Paths.Remove(deleted);
            }

            try {
                await _context.SaveChangesAsync();
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

            _context.Templates.Add(unpacked.Item1);

            await _context.SaveChangesAsync();

            TemplateOut templateRet = TemplateToTemplateOut(unpacked.Item1);

            foreach(Path p in unpacked.Item2) {
                p.TemplateID = templateRet.ID;
                _context.Paths.Add(p);
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTemplate", new { id = templateRet.ID }, templateRet);
        }

        // DELETE: api/Templates/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<TemplateOut>> DeleteTemplate(int id) {
            var template = await _context.Templates.FindAsync(id);
            if (template == null) {
                return NotFound();
            }

            TemplateOut templateOut = TemplateToTemplateOut(template);

            foreach(Path p in GetPaths(template.ID))
                _context.Paths.Remove(p);

            var jobs = _context.Jobs.FromSqlRaw(@"
                SELECT *
                FROM Jobs j
                WHERE TemplateID = " + id
            );

            await jobs.ForEachAsync(job => {
                var logs = _context.Log.FromSqlRaw(@"
                    SELECT *
                    FROM Logs l
                    WHERE JobID = " + job.ID
                );
                _context.Log.RemoveRange(logs);
            });

            _context.Jobs.RemoveRange(jobs);
            await _context.SaveChangesAsync();

            _context.Templates.Remove(template);
            await _context.SaveChangesAsync();

            return templateOut;
        }

        private bool TemplateExists(int id) {
            return _context.Templates.Any(e => e.ID == id);
        }

        private TemplateOut TemplateToTemplateOut(Template template) {
            TemplateOut tOut = TemplateOut.FromTemplate(template);

            foreach (Path path in GetPaths(template.ID)) {
                if (path.Source)
                    tOut.Sources.Add(PathOut.FromPath(path));
                else 
                    tOut.Targets.Add(PathOut.FromPath(path));
            }

            return tOut;
        }

        private Path[] GetPaths(int templateID) {
            return _context.Paths.FromSqlRaw(@"
                SELECT *
                FROM Paths p
                WHERE TemplateID = " + templateID
            ).ToArray();
        }
        
        private (Template, List<Path>) TemplateOutToTemplateAndPaths(TemplateOut templateOut) {
            Template template = templateOut.ToTemplate();
            List<Path> paths = new List<Path>();

            foreach(PathOut p in templateOut.Sources)
                paths.Add(p.ToPath(template.ID, true));
                
            foreach(PathOut p in templateOut.Targets)
                paths.Add(p.ToPath(template.ID, false));

            return (template, paths);
        } 
    }
}
