using Microsoft.EntityFrameworkCore;
using Needle.Domain.Albums;
using Needle.Domain.Reviews;

namespace Needle.Infrastructure.Persistence;

public sealed class NeedleDbContext(
    DbContextOptions<NeedleDbContext> options)
    : DbContext(options)
{
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(NeedleDbContext).Assembly);
    }
}