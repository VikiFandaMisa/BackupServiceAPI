using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BackupServiceAPI.Models
{
    public class DbBackupServiceContext : DbContext
    {
        public DbBackupServiceContext(DbContextOptions<DbBackupServiceContext> options) : base(options) { }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<LogItem> Log { get; set; }
        public DbSet<Computer> Computers { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<Path> Paths { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<InvalidatedToken> TokenBlacklist { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            // Enum conversions

            // Computer
            modelBuilder
                .Entity<Computer>()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<ComputerStatus>());
            
            // Template
            modelBuilder
                .Entity<Template>()
                .Property(e => e.Type)
                .HasConversion(new EnumToStringConverter<BackupType>());

            modelBuilder
                .Entity<Template>()
                .Property(e => e.TargetFileType)
                .HasConversion(new EnumToStringConverter<BackupFileType>());
            
            // Log
            modelBuilder
                .Entity<LogItem>()
                .Property(e => e.Type)
                .HasConversion(new EnumToStringConverter<MessageType>());
        }
    }
}