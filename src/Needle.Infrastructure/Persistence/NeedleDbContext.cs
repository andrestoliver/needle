using Microsoft.EntityFrameworkCore;
using Needle.Domain.Albums;

namespace Needle.Infrastructure.Persistence;

public sealed class NeedleDbContext(
    DbContextOptions<NeedleDbContext> options)
    : DbContext(options)
{
    public DbSet<Album> Albums => Set<Album>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(NeedleDbContext).Assembly);
    }
}