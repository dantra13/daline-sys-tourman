using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sport.Core.Competitions;

namespace Sport.Infrastructure.Configurations.Competitions;

internal sealed class CompetitionConfiguration : IEntityTypeConfiguration<Competition>
{
    public void Configure(EntityTypeBuilder<Competition> b)
    {
        b.ToTable("competitions");

        b.HasKey(c => c.Id);
        b.Property(c => c.Id).HasVogenConversion();

        b.Property(c => c.Code)
            .HasVogenConversion()
            .HasMaxLength(64)
            .IsRequired();
        b.HasIndex(c => c.Code).IsUnique();

        b.Property(c => c.Name).HasMaxLength(200).IsRequired();

        b.ComplexProperty(c => c.Dates, dr =>
        {
            dr.Property(d => d.Start).HasColumnName("dates_start").IsRequired();
            dr.Property(d => d.End)  .HasColumnName("dates_end")  .IsRequired();
        });

        b.HasMany(c => c.Disciplines)
            .WithOne()
            .HasForeignKey(d => d.CompetitionId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Navigation(c => c.Disciplines)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
