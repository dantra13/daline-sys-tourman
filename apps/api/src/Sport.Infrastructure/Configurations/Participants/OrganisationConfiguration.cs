using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sport.Core.Participants;

namespace Sport.Infrastructure.Configurations.Participants;

internal sealed class OrganisationConfiguration : IEntityTypeConfiguration<Organisation>
{
    public void Configure(EntityTypeBuilder<Organisation> b)
    {
        b.ToTable("organisations");

        b.HasKey(o => o.Id);
        b.Property(o => o.Id).HasVogenConversion();

        b.Property(o => o.Code)
            .HasVogenConversion()
            .HasMaxLength(10).IsRequired();
        b.HasIndex(o => o.Code).IsUnique();

        b.Property(o => o.Name).HasMaxLength(200).IsRequired();
        b.Property(o => o.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
    }
}
