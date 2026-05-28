using Sport.Core.DisciplineRegistry;
using Sport.Core.Shared;

namespace Sport.Core.Competitions;

public sealed class Competition
{
    public CompetitionId Id { get; }
    public CompetitionCode Code { get; }
    public string Name { get; }
    public DateRange Dates { get; }

    private readonly List<CompetitionDiscipline> _disciplines;
    public IReadOnlyList<CompetitionDiscipline> Disciplines => _disciplines;

    private Competition(
        CompetitionId id,
        CompetitionCode code,
        string name,
        DateRange dates,
        List<CompetitionDiscipline> disciplines)
    {
        Id = id;
        Code = code;
        Name = name;
        Dates = dates;
        _disciplines = disciplines;
    }

    public static Competition Create(
        CompetitionId id,
        CompetitionCode code,
        string name,
        DateRange dates,
        IReadOnlyCollection<(DisciplineCode Code, IReadOnlySet<GenderCode> Genders)> disciplines,
        IDisciplineRegistry registry)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Competition.Name is required.");
        if (disciplines is null || disciplines.Count < 1)
            throw new DomainException("A Competition must have at least 1 discipline (I-COMP-1).");

        var seen = new HashSet<DisciplineCode>();
        var children = new List<CompetitionDiscipline>(disciplines.Count);

        foreach (var (disciplineCode, genders) in disciplines)
        {
            if (!seen.Add(disciplineCode))
                throw new DomainException($"Duplicate discipline '{disciplineCode.Value}' in competition (I-COMP-3).");

            if (!registry.IsRegistered(disciplineCode))
                throw new DomainException($"Discipline '{disciplineCode.Value}' is not registered in the DisciplineRegistry (I-COMP-2).");

            var module = registry.Get(disciplineCode);
            foreach (var g in genders)
                if (!module.SupportedGenders.Contains(g))
                    throw new DomainException($"Gender '{g}' is not supported by discipline '{disciplineCode.Value}' (I-COMP-4).");

            children.Add(CompetitionDiscipline.Create(
                CompetitionDisciplineId.New(),
                id,
                disciplineCode,
                genders));
        }

        return new Competition(id, code, name, dates, children);
    }
}
