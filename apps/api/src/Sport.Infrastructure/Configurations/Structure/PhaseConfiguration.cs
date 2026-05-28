using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sport.Core.Structure;

namespace Sport.Infrastructure.Configurations.Structure;

internal sealed class PhaseConfiguration : IEntityTypeConfiguration<Phase>
{
    public void Configure(EntityTypeBuilder<Phase> b)
    {
        b.ToTable("phases");

        b.HasKey(p => p.Id);
        b.Property(p => p.Id).HasVogenConversion();

        b.Property(p => p.EventId).HasVogenConversion().IsRequired();

        b.Property(p => p.Code)
            .HasVogenConversion()
            .HasMaxLength(4).IsRequired();

        b.Property(p => p.Order).IsRequired();

        b.Property(p => p.Rsc)
            .HasVogenConversion()
            .HasMaxLength(34).IsRequired();

        b.HasIndex(p => new { p.EventId, p.Code }).IsUnique();
        b.HasIndex(p => new { p.EventId, p.Order }).IsUnique();

        b.HasMany<Unit>("_units")
            .WithOne()
            .HasForeignKey(u => u.PhaseId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Metadata.FindNavigation("_units")!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
