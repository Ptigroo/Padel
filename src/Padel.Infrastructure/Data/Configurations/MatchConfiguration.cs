using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Padel.Domain.Entities;

namespace Padel.Infrastructure.Data.Configurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.ScheduledAt).IsRequired();
        builder.Property(m => m.EndsAt).IsRequired();

        builder.Property(m => m.MatchType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(m => m.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(15);

        builder.HasOne(m => m.Court)
            .WithMany(c => c.Matches)
            .HasForeignKey(m => m.CourtId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Organizer)
            .WithMany(mem => mem.OrganizedMatches)
            .HasForeignKey(m => m.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
