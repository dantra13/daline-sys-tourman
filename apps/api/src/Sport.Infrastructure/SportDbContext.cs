using Microsoft.EntityFrameworkCore;
using Sport.Core.Competitions;
using Sport.Core.Officials;
using Sport.Core.Participants;
using Sport.Core.Structure;

namespace Sport.Infrastructure;

public sealed class SportDbContext(DbContextOptions<SportDbContext> options) : DbContext(options)
{
    public DbSet<Competition>        Competitions        => Set<Competition>();
    public DbSet<Event>              Events              => Set<Event>();
    public DbSet<Phase>              Phases              => Set<Phase>();
    public DbSet<Unit>               Units               => Set<Unit>();
    public DbSet<Subunit>            Subunits            => Set<Subunit>();
    public DbSet<Person>             Persons             => Set<Person>();
    public DbSet<Organisation>       Organisations       => Set<Organisation>();
    public DbSet<Team>               Teams               => Set<Team>();
    public DbSet<Entry>              Entries             => Set<Entry>();
    public DbSet<OfficialAssignment> OfficialAssignments => Set<OfficialAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SportDbContext).Assembly);
    }
}
