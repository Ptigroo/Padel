using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Padel.Domain.Entities;

namespace Padel.Infrastructure.Data.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Matricule)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(m => m.Matricule)
            .IsUnique();

        builder.Property(m => m.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.MemberType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.HasOne(m => m.Site)
            .WithMany(s => s.Members)
            .HasForeignKey(m => m.SiteId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
