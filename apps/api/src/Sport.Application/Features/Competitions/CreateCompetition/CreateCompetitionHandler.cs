using Sport.Application.Abstractions;
using Sport.Application.Common;
using Sport.Application.Features.Competitions;
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;

namespace Sport.Application.Features.Competitions.CreateCompetition;

public static class CreateCompetitionHandler
{
    public static async Task<CompetitionDto> Handle(
        CreateCompetition cmd,
        ICompetitionRepository repo,
        IDisciplineRegistry registry,
        IUnitOfWork uow,
        CancellationToken ct)
    {
        var failures = new List<ValidationFailure>();

        // 1. Surface guards.
        if (string.IsNullOrWhiteSpace(cmd.Name))
            failures.Add(new ValidationFailure("competition.name_required", "name"));

        var codeValid = CompetitionCode.TryFrom(cmd.Code, out var code);
        if (!codeValid)
            failures.Add(new ValidationFailure("competition.code_invalid", "code"));

        if (cmd.StartDate > cmd.EndDate)
            failures.Add(new ValidationFailure(
                "competition.date_range_invalid",
                "dates",
                new Dictionary<string, object?>
                {
                    ["start"] = cmd.StartDate.ToString("yyyy-MM-dd"),
                    ["end"]   = cmd.EndDate.ToString("yyyy-MM-dd"),
                }));

        if (cmd.Disciplines is null || cmd.Disciplines.Count == 0)
            failures.Add(new ValidationFailure("competition.disciplines_required", "disciplines"));

        var disciplineInputs = new List<(DisciplineCode Code, IReadOnlySet<GenderCode> Genders)>();
        if (cmd.Disciplines is not null)
        {
            for (var i = 0; i < cmd.Disciplines.Count; i++)
            {
                var item = cmd.Disciplines[i];
                if (!DisciplineCode.TryFrom(item.Code, out var disciplineCode))
                {
                    failures.Add(new ValidationFailure(
                        "competition.discipline_code_invalid",
                        $"disciplines[{i}].code"));
                    continue;
                }

                if (item.Genders is null || item.Genders.Count == 0)
                {
                    failures.Add(new ValidationFailure(
                        "competition.genders_required",
                        $"disciplines[{i}].genders"));
                    continue;
                }

                var genders = new HashSet<GenderCode>();
                for (var g = 0; g < item.Genders.Count; g++)
                {
                    if (!Enum.TryParse<GenderCode>(item.Genders[g], ignoreCase: true, out var gender))
                    {
                        failures.Add(new ValidationFailure(
                            "competition.gender_code_invalid",
                            $"disciplines[{i}].genders[{g}]"));
                    }
                    else
                    {
                        genders.Add(gender);
                    }
                }

                disciplineInputs.Add((disciplineCode, genders));
            }
        }

        if (failures.Count > 0)
            throw new ValidationException(failures);

        // 2. Conflict check.
        if (await repo.ExistsByCodeAsync(code, ct))
        {
            throw new ValidationException(new ValidationFailure(
                "competition.code_already_exists",
                "code",
                new Dictionary<string, object?> { ["code"] = cmd.Code }));
        }

        // 3. Build the aggregate (may throw DomainException).
        var dateRange = DateRange.Create(cmd.StartDate, cmd.EndDate);
        var competition = Competition.Create(
            id: CompetitionId.From(Guid.NewGuid()),
            code: code,
            name: cmd.Name,
            dates: dateRange,
            disciplines: disciplineInputs,
            registry: registry);

        // 4. Persist.
        await repo.AddAsync(competition, ct);
        await uow.SaveChangesAsync(ct);

        return CompetitionDto.From(competition);
    }
}
