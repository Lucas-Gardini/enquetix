using Microsoft.EntityFrameworkCore;

namespace enquetix.Modules.Application.EntityFramework
{
    public class Context(DbContextOptions<Context> options) : DbContext(options)
    {
        //public DbSet<Blog> Blogs { get; set; }
    }
}
