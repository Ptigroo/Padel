using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Padel.Domain.Entities;

namespace Padel.Infrastructure.Data.Configurations;

public class ClosureDayConfiguration : IEntityTypeConfiguration<ClosureDay>
{
    public void Configure(EntityTypeBuilder<ClosureDay> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Date)
            .IsRequired();

        builder.Property(c => c.Reason)
            .HasMaxLength(250);

        builder.HasOne(c => c.Site)
            .WithMany(s => s.ClosureDays)
            .HasForeignKey(c => c.SiteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
