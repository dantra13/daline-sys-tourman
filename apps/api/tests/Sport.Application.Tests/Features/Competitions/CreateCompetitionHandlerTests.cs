using FluentAssertions;
using Sport.Application.Common;
using Sport.Application.Features.Competitions.CreateCompetition;
using Sport.Application.Tests.Fakes;
using Sport.Core.Shared;

namespace Sport.Application.Tests.Features.Competitions;

public class CreateCompetitionHandlerTests
{
    private static CreateCompetition ValidCommand() => new(
        Code: "jud-2026",
        Name: "Judo Open 2026",
        StartDate: new DateOnly(2026, 8, 1),
        EndDate: new DateOnly(2026, 8, 5),
        Disciplines: new[]
        {
            new CreateCompetition.DisciplineInput("FBL", new[] { "M" }),
        });

    [Fact]
    public async Task Happy_path_persists_and_returns_dto()
    {
        var repo = new InMemoryCompetitionRepository();
        var uow = new NoopUnitOfWork();
        var registry = TestRegistry.WithFblM();

        var dto = await CreateCompetitionHandler.Handle(
            ValidCommand(), repo, registry, uow, CancellationToken.None);

        dto.Code.Should().Be("jud-2026");
        dto.Disciplines.Should().HaveCount(1);
        repo.Snapshot.Should().HaveCount(1);
        uow.SaveCalls.Should().Be(1);
    }

    [Fact]
    public async Task Blank_name_throws_ValidationException_with_code()
    {
        var repo = new InMemoryCompetitionRepository();
        var uow = new NoopUnitOfWork();
        var registry = TestRegistry.WithFblM();
        var cmd = ValidCommand() with { Name = "   " };

        Func<Task> act = () => CreateCompetitionHandler.Handle(
            cmd, repo, registry, uow, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Failures.Should().ContainSingle(f => f.Code == "competition.name_required");
        repo.Snapshot.Should().BeEmpty();
        uow.SaveCalls.Should().Be(0);
    }

    [Fact]
    public async Task Invalid_code_format_throws_ValidationException_with_code_invalid()
    {
        var repo = new InMemoryCompetitionRepository();
        var uow = new NoopUnitOfWork();
        var registry = TestRegistry.WithFblM();
        var cmd = ValidCommand() with { Code = "INVALID-Has-Upper" };

        Func<Task> act = () => CreateCompetitionHandler.Handle(
            cmd, repo, registry, uow, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Failures.Should().ContainSingle(f => f.Code == "competition.code_invalid");
    }

    [Fact]
    public async Task End_before_start_throws_ValidationException_with_date_range_invalid()
    {
        var repo = new InMemoryCompetitionRepository();
        var uow = new NoopUnitOfWork();
        var registry = TestRegistry.WithFblM();
        var cmd = ValidCommand() with
        {
            StartDate = new DateOnly(2026, 8, 10),
            EndDate = new DateOnly(2026, 8, 1),
        };

        Func<Task> act = () => CreateCompetitionHandler.Handle(
            cmd, repo, registry, uow, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Failures.Should().ContainSingle(f => f.Code == "competition.date_range_invalid");
    }

    [Fact]
    public async Task Duplicate_code_throws_ValidationException_with_code_already_exists()
    {
        var repo = new InMemoryCompetitionRepository();
        var uow = new NoopUnitOfWork();
        var registry = TestRegistry.WithFblM();
        await repo.AddAsync(CompetitionFactory.JudOpen(registry), CancellationToken.None);

        Func<Task> act = () => CreateCompetitionHandler.Handle(
            ValidCommand(), repo, registry, uow, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Failures.Should().ContainSingle(f => f.Code == "competition.code_already_exists");
    }

    [Fact]
    public async Task Domain_invariant_violation_propagates_DomainException()
    {
        var repo = new InMemoryCompetitionRepository();
        var uow = new NoopUnitOfWork();
        var registry = TestRegistry.WithFblM();
        // ATH is not in the test registry (only FBL is) -> I-COMP-2.
        var cmd = ValidCommand() with
        {
            Disciplines = new[]
            {
                new CreateCompetition.DisciplineInput("ATH", new[] { "M" }),
            },
        };

        Func<Task> act = () => CreateCompetitionHandler.Handle(
            cmd, repo, registry, uow, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<DomainException>();
        ex.Which.Code.Should().Be("I-COMP-2");
    }
}
