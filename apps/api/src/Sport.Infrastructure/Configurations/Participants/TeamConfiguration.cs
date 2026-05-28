using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sport.Core.Participants;

namespace Sport.Infrastructure.Configurations.Participants;

internal sealed class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> b)
    {
        b.ToTable("teams");

        b.HasKey(t => t.Id);
        b.Property(t => t.Id).HasVogenConversion();

        b.Property(t => t.Code)
            .HasVogenConversion()
            .HasMaxLength(20).IsRequired();
        b.HasIndex(t => t.Code).IsUnique();

        b.Property(t => t.Name).HasMaxLength(200).IsRequired();

        b.Property(t => t.OrganisationId).HasVogenConversion().IsRequired();

        b.Property(t => t.DisciplineCode)
            .HasVogenConversion()
            .HasMaxLength(3).IsRequired();
    }
}
