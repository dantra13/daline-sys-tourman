using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sport.Core.Structure;

namespace Sport.Infrastructure.Configurations.Structure;

internal sealed class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> b)
    {
        b.ToTable("events");

        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasVogenConversion();

        b.Property(e => e.CompetitionDisciplineId).HasVogenConversion().IsRequired();

        b.Property(e => e.DisciplineCode)
            .HasVogenConversion()
            .HasMaxLength(3).IsRequired();

        b.Property(e => e.Gender).HasConversion<string>().HasMaxLength(1).IsRequired();

        b.Property(e => e.EventType)
            .HasVogenConversion()
            .HasMaxLength(8).IsRequired();

        b.Property(e => e.EventModifier)
            .HasConversion(
                v => v.HasValue ? v.Value.Value : (string?)null,
                v => v != null ? EventModifierCode.From(v) : (EventModifierCode?)null)
            .HasMaxLength(10);

        b.Property(e => e.Name).HasMaxLength(200).IsRequired();

        b.Property(e => e.Rsc)
            .HasVogenConversion()
            .HasMaxLength(34).IsRequired();
        b.HasIndex(e => new { e.CompetitionDisciplineId, e.Rsc }).IsUnique();

        b.HasMany<Phase>("_phases")
            .WithOne()
            .HasForeignKey(p => p.EventId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Metadata.FindNavigation("_phases")!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
