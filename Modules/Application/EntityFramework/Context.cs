using enquetix.Modules.User.Repository;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace enquetix.Modules.Application.EntityFramework
{
    public class Context(DbContextOptions<Context> options) : DbContext(options)
    {
        public DbSet<UserModel> Users => Set<UserModel>();

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
