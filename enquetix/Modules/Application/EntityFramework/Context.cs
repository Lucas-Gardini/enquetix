using enquetix.Modules.AuditLog.Repository;
using enquetix.Modules.AuditLog.Services;
using enquetix.Modules.Auth.Services;
using enquetix.Modules.Poll.Repository;
using enquetix.Modules.User.Repository;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace enquetix.Modules.Application.EntityFramework
{
    public class Context(DbContextOptions<Context> options, IServiceProvider serviceProvider) : DbContext(options)
    {
        public DbSet<UserModel> Users => Set<UserModel>();
        public DbSet<PollModel> Polls => Set<PollModel>();
        public DbSet<PollVoteModel> PollVotes => Set<PollVoteModel>();
        public DbSet<PollOptionModel> PollOptions => Set<PollOptionModel>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var genericEntities = modelBuilder.Model.GetEntityTypes()
                .Where(entity => typeof(GenericModel).IsAssignableFrom(entity.ClrType));

            foreach (var entity in genericEntities.Select(e => e.ClrType))
            {
                modelBuilder.Entity(entity).Property("CreatedAt")
                    .HasDefaultValueSql("TIMEZONE('UTC', CURRENT_TIMESTAMP)");

                modelBuilder.Entity(entity).Property("UpdatedAt")
                    .HasDefaultValueSql("TIMEZONE('UTC', CURRENT_TIMESTAMP)");
            }

            // Poll ➜ PollOptions
            modelBuilder.Entity<PollModel>()
                .HasMany(p => p.Options)
                .WithOne(o => o.Poll)
                .HasForeignKey(o => o.PollId)
                .OnDelete(DeleteBehavior.Cascade);

            // Poll ➜ PollVotes
            modelBuilder.Entity<PollVoteModel>()
                .HasOne(v => v.Poll)
                .WithMany()
                .HasForeignKey(v => v.PollId)
                .OnDelete(DeleteBehavior.Cascade);

            // Option ➜ PollVotes
            modelBuilder.Entity<PollVoteModel>()
                .HasOne(v => v.Option)
                .WithMany()
                .HasForeignKey(v => v.OptionId)
                .OnDelete(DeleteBehavior.Cascade);

            // User ➜ PollVotes
            modelBuilder.Entity<PollVoteModel>()
                .HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public override int SaveChanges()
        {
            var entries = ChangeTracker.Entries<GenericModel>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }

                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }

            LogChanges().Wait();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<GenericModel>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }

                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }

            await LogChanges();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private async Task LogChanges()
        {
            using var scope = serviceProvider.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var auditLogService = scope.ServiceProvider.GetRequiredService<IAuditLogService>();

            var loggedUserId = authService.GetLoggedUserIdSafe();

            var changeTracker = this.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted);

            foreach (var entry in changeTracker)
            {
                LogOperation operation = entry.State switch
                {
                    EntityState.Added => LogOperation.Insert,
                    EntityState.Modified => LogOperation.Update,
                    EntityState.Deleted => LogOperation.Delete,
                    _ => throw new InvalidOperationException("Unsupported entity state for logging.")
                };

                var log = new LogModel
                {
                    EntityName = entry.Entity.GetType().Name,
                    EntityId = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString()!,
                    Operation = operation,
                    Timestamp = DateTime.UtcNow,
                    User = loggedUserId?.ToString() ?? "anon",
                    OldValues = [],
                    NewValues = []
                };

                foreach (var prop in entry.Properties)
                {
                    var propName = prop.Metadata.Name;

                    static object? ToSafeMongoValue(object? val)
                    {
                        if (val is Guid guidVal)
                            return guidVal.ToString();
                        return val;
                    }

                    if (entry.State == EntityState.Modified)
                    {
                        log.OldValues[propName] = ToSafeMongoValue(prop.OriginalValue);
                        log.NewValues[propName] = ToSafeMongoValue(prop.CurrentValue);
                    }
                    else if (entry.State == EntityState.Added)
                    {
                        log.NewValues[propName] = ToSafeMongoValue(prop.CurrentValue);
                    }
                    else if (entry.State == EntityState.Deleted)
                    {
                        log.OldValues[propName] = ToSafeMongoValue(prop.OriginalValue);
                    }
                }
                await auditLogService.SaveLog(log);
            }
        }
    }

    public partial class GenericModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
