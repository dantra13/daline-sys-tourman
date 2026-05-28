using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sport.Core.Participants;

namespace Sport.Infrastructure.Configurations.Participants;

internal sealed class EntryConfiguration : IEntityTypeConfiguration<Entry>
{
    public void Configure(EntityTypeBuilder<Entry> b)
    {
        b.ToTable("entries");

        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasVogenConversion();

        b.Property(e => e.EventId).HasVogenConversion().IsRequired();
        b.Property(e => e.Type).HasConversion<string>().HasMaxLength(10).IsRequired();
        b.Property(e => e.OrganisationId).HasVogenConversion().IsRequired();

        // Nullable VO — explicit lambdas
        b.Property(e => e.TeamId)
            .HasConversion(
                v => v.HasValue ? v.Value.Value : (Guid?)null,
                v => v != null ? TeamId.From(v.Value) : (TeamId?)null);

        b.Property(e => e.Bib)
            .HasConversion(
                v => v.HasValue ? v.Value.Value : (string?)null,
                v => v != null ? Bib.From(v) : (Bib?)null)
            .HasMaxLength(20);

        b.Property(e => e.Seed);
        b.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        b.HasIndex(e => new { e.EventId, e.Status });

        b.HasMany(e => e.Composition)
            .WithOne()
            .HasForeignKey(m => m.EntryId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(e => e.Composition)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
