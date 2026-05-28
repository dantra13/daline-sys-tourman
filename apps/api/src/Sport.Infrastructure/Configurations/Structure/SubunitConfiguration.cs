using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sport.Core.Structure;

namespace Sport.Infrastructure.Configurations.Structure;

internal sealed class SubunitConfiguration : IEntityTypeConfiguration<Subunit>
{
    public void Configure(EntityTypeBuilder<Subunit> b)
    {
        b.ToTable("subunits");

        b.HasKey(s => s.Id);
        b.Property(s => s.Id).HasVogenConversion();

        b.Property(s => s.UnitId).HasVogenConversion().IsRequired();

        b.Property(s => s.Code)
            .HasVogenConversion()
            .HasMaxLength(2).IsRequired();

        b.Property(s => s.Rsc)
            .HasVogenConversion()
            .HasMaxLength(34).IsRequired();

        b.HasIndex(s => new { s.UnitId, s.Code }).IsUnique();
    }
}
