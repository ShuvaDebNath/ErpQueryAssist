using ErpQueryAssist.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ErpQueryAssist.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<UserQuery> UserQueries => Set<UserQuery>();
    public DbSet<UserQueryResult> UserQueryResults => Set<UserQueryResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserQuery>()
            .HasMany(q => q.Results)
            .WithOne(r => r.UserQuery)
            .HasForeignKey(r => r.UserQueryId);
    }
}
