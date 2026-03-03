using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Padel.Domain.Entities;

namespace Padel.Infrastructure.Data.Configurations;

public class SiteScheduleConfiguration : IEntityTypeConfiguration<SiteSchedule>
{
    public void Configure(EntityTypeBuilder<SiteSchedule> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Year)
            .IsRequired();

        builder.Property(s => s.StartTime)
            .IsRequired();

        builder.Property(s => s.EndTime)
            .IsRequired();

        builder.HasIndex(s => new { s.SiteId, s.Year })
            .IsUnique();

        builder.HasOne(s => s.Site)
            .WithMany(site => site.Schedules)
            .HasForeignKey(s => s.SiteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
