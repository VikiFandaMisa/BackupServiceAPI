using Microsoft.EntityFrameworkCore;

namespace BackupServiceAPI.Models
{
    public class BackupDBContext : DbContext
    {
        public DbSet<Job> Jobs { get; set; }
        public DbSet<LogRecord> Log { get; set; }
        public DbSet<Computer> Computers { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<Path> Paths { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public BackupDBContext(DbContextOptions<BackupDBContext> options) : base(options) {

        }
    }
}