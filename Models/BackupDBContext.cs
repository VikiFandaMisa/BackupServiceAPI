using Microsoft.EntityFrameworkCore;

namespace BackupServiceAPI.Models
{
    public class BackupDBContext : DbContext
    {
        public BackupDBContext(DbContextOptions<BackupDBContext> options) : base(options) { }

        /*
        // Controller generation fix
        public BackupDBContext() {}
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("BackupDB");
        }
        */
        
        public DbSet<Job> Jobs { get; set; }
        public DbSet<LogRecord> Log { get; set; }
        public DbSet<Computer> Computers { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<Path> Paths { get; set; }
        public DbSet<Account> Accounts { get; set; }
    }
}