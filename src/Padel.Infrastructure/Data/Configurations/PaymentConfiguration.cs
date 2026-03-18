using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Padel.Domain.Entities;

namespace Padel.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(p => p.CreatedAt).IsRequired();

        builder.HasOne(p => p.MatchPlayer)
            .WithOne(mp => mp.Payment)
            .HasForeignKey<Payment>(p => p.MatchPlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Match)
            .WithMany(m => m.Payments)
            .HasForeignKey(p => p.MatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Member)
            .WithMany(m => m.Payments)
            .HasForeignKey(p => p.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
