using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Padel.Domain.Entities;

namespace Padel.Infrastructure.Data.Configurations;

public class MatchPlayerConfiguration : IEntityTypeConfiguration<MatchPlayer>
{
    public void Configure(EntityTypeBuilder<MatchPlayer> builder)
    {
        builder.HasKey(mp => mp.Id);

        builder.Property(mp => mp.JoinedAt).IsRequired();

        builder.HasIndex(mp => new { mp.MatchId, mp.MemberId })
            .IsUnique();

        builder.HasOne(mp => mp.Match)
            .WithMany(m => m.Players)
            .HasForeignKey(mp => mp.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mp => mp.Member)
            .WithMany(m => m.MatchPlayers)
            .HasForeignKey(mp => mp.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
