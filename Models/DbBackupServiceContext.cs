using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BackupServiceAPI.Models
{
    public class DbBackupServiceContext : DbContext
    {
        public DbBackupServiceContext(DbContextOptions<DbBackupServiceContext> options) : base(options) { }

        /*
        // Controller generation fix
        public DbBackupServiceContext() {}
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