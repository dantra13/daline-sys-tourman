using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sport.Core.Officials;
using Sport.Core.Participants;

namespace Sport.Infrastructure.Configurations.Officials;

internal sealed class OfficialAssignmentConfiguration : IEntityTypeConfiguration<OfficialAssignment>
{
    public void Configure(EntityTypeBuilder<OfficialAssignment> b)
    {
        b.ToTable("official_assignments");

        b.HasKey(a => a.Id);
        b.Property(a => a.Id).HasVogenConversion();

        b.Property(a => a.PersonId).HasVogenConversion().IsRequired();

        b.Property(a => a.FunctionCode)
            .HasVogenConversion()
            .HasMaxLength(20).IsRequired();

        b.ComplexProperty(a => a.Scope, sb =>
        {
            sb.Property(s => s.Level).HasConversion<string>().HasMaxLength(30).IsRequired().HasColumnName("scope_level");
            sb.Property(s => s.TargetId).IsRequired().HasColumnName("scope_target_id");
        });

        b.Property(a => a.OrganisationId)
            .HasConversion(
                v => v.HasValue ? v.Value.Value : (Guid?)null,
                v => v != null ? OrganisationId.From(v.Value) : (OrganisationId?)null);

        b.Property(a => a.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        // Note: composite index over (scope_level, scope_target_id, function_code) is added
        // directly in the migration via migrationBuilder.CreateIndex because EF Core's HasIndex
        // does not support ComplexProperty member paths.
    }
}
