using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sport.Core.Structure;

namespace Sport.Infrastructure.Configurations.Structure;

internal sealed class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> b)
    {
        b.ToTable("units");

        b.HasKey(u => u.Id);
        b.Property(u => u.Id).HasVogenConversion();

        b.Property(u => u.PhaseId).HasVogenConversion().IsRequired();

        b.Property(u => u.Code)
            .HasVogenConversion()
            .HasMaxLength(8).IsRequired();

        b.Property(u => u.ScheduledStart);

        b.Property(u => u.Rsc)
            .HasVogenConversion()
            .HasMaxLength(34).IsRequired();

        b.Property(u => u.DisciplineUnitRef);

        b.HasIndex(u => new { u.PhaseId, u.Code }).IsUnique();

        b.HasMany<Subunit>("_subunits")
            .WithOne()
            .HasForeignKey(s => s.UnitId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Metadata.FindNavigation("_subunits")!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
