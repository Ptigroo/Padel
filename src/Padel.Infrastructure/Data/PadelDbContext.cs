using Microsoft.EntityFrameworkCore;
using Padel.Domain.Entities;

namespace Padel.Infrastructure.Data;

public class PadelDbContext(DbContextOptions<PadelDbContext> options) : DbContext(options)
{
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<Court> Courts => Set<Court>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PadelDbContext).Assembly);
    }
}
