using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;

namespace Sport.Infrastructure.Configurations.Competitions;

internal sealed class CompetitionDisciplineConfiguration : IEntityTypeConfiguration<CompetitionDiscipline>
{
    public void Configure(EntityTypeBuilder<CompetitionDiscipline> b)
    {
        b.ToTable("competition_disciplines");

        b.HasKey(d => d.Id);
        b.Property(d => d.Id).HasVogenConversion();

        b.Property(d => d.CompetitionId).HasVogenConversion().IsRequired();

        b.Property(d => d.Code)
            .HasVogenConversion()
            .HasMaxLength(3)
            .IsRequired();

        b.Property(d => d.EnabledGenders)
            .HasConversion(
                v => v.Select(g => g.ToString()).ToArray(),
                v => (IReadOnlyList<GenderCode>)v.Select(s => Enum.Parse<GenderCode>(s)).ToArray())
            .Metadata
            .SetValueComparer(new ValueComparer<IReadOnlyList<GenderCode>>(
                (a, b) => a != null && b != null && a.SequenceEqual(b),
                v => v.Aggregate(0, (h, e) => HashCode.Combine(h, e.GetHashCode())),
                v => (IReadOnlyList<GenderCode>)v.ToArray()));

        b.Property(d => d.EnabledGenders)
            .HasColumnType("text[]")
            .HasColumnName("enabled_genders")
            .IsRequired();

        b.HasIndex(d => new { d.CompetitionId, d.Code }).IsUnique();
    }
}
