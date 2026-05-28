using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Participants;

namespace Sport.Infrastructure.Configurations.Participants;

internal sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> b)
    {
        b.ToTable("persons");

        b.HasKey(p => p.Id);
        b.Property(p => p.Id).HasVogenConversion();

        b.Property(p => p.FamilyName).HasMaxLength(50).IsRequired();
        b.Property(p => p.GivenName).HasMaxLength(50);
        b.Property(p => p.Gender).HasConversion<string>().HasMaxLength(1).IsRequired();
        b.Property(p => p.BirthDate);
        b.Property(p => p.IFId).HasMaxLength(20);

        b.HasIndex(p => new { p.FamilyName, p.GivenName });
    }
}
