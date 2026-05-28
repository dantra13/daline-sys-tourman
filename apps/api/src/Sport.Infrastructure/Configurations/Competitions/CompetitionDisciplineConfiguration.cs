using Microsoft.EntityFrameworkCore;
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
                v => v.Select(s => Enum.Parse<GenderCode>(s)).ToHashSet())
            .HasColumnType("text[]")
            .HasColumnName("enabled_genders")
            .IsRequired();

        b.HasIndex(d => new { d.CompetitionId, d.Code }).IsUnique();
    }
}
