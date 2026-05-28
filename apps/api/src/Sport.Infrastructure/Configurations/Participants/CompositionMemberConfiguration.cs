using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sport.Core.Participants;

namespace Sport.Infrastructure.Configurations.Participants;

internal sealed class CompositionMemberConfiguration : IEntityTypeConfiguration<CompositionMember>
{
    public void Configure(EntityTypeBuilder<CompositionMember> b)
    {
        b.ToTable("composition_members");

        b.HasKey(m => new { m.EntryId, m.PersonId });

        b.Property(m => m.EntryId).HasVogenConversion();
        b.Property(m => m.PersonId).HasVogenConversion();
        b.Property(m => m.Order).IsRequired();

        b.Property(m => m.Bib)
            .HasConversion(
                v => v.HasValue ? v.Value.Value : (string?)null,
                v => v != null ? Bib.From(v) : (Bib?)null)
            .HasMaxLength(20);

        b.HasIndex(m => new { m.PersonId, m.EntryId }).IsUnique();
    }
}
