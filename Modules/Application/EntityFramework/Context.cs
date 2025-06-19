using enquetix.Modules.Poll.Repository;
using enquetix.Modules.User.Repository;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace enquetix.Modules.Application.EntityFramework
{
    public class Context(DbContextOptions<Context> options) : DbContext(options)
    {
        public DbSet<UserModel> Users => Set<UserModel>();
        public DbSet<PollModel> Polls => Set<PollModel>();
        public DbSet<PollVotesModel> PollVotes => Set<PollVotesModel>();
        public DbSet<PollOptionsModel> PollOptions => Set<PollOptionsModel>();

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
            modelBuilder.Entity<PollVotesModel>()
                .HasOne(v => v.Poll)
                .WithMany()
                .HasForeignKey(v => v.PollId)
                .OnDelete(DeleteBehavior.Cascade);

            // Option ➜ PollVotes
            modelBuilder.Entity<PollVotesModel>()
                .HasOne(v => v.Option)
                .WithMany()
                .HasForeignKey(v => v.OptionId)
                .OnDelete(DeleteBehavior.Cascade);

            // User ➜ PollVotes
            modelBuilder.Entity<PollVotesModel>()
                .HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);
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

            return await base.SaveChangesAsync(cancellationToken);
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
