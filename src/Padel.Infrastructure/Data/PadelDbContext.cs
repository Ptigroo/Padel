using Microsoft.EntityFrameworkCore;
using Padel.Domain.Entities;

namespace Padel.Infrastructure.Data;

public class PadelDbContext(DbContextOptions<PadelDbContext> options) : DbContext(options)
{
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<Court> Courts => Set<Court>();
    public DbSet<SiteSchedule> SiteSchedules => Set<SiteSchedule>();
    public DbSet<ClosureDay> ClosureDays => Set<ClosureDay>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchPlayer> MatchPlayers => Set<MatchPlayer>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Administrator> Administrators => Set<Administrator>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PadelDbContext).Assembly);
    }
}
