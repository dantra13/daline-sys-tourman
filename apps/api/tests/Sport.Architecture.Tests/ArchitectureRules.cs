using FluentAssertions;
using NetArchTest.Rules;
using Sport.Api.ErrorHandling;
using Sport.Application;
using Sport.Application.Abstractions;
using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Shared;

namespace Sport.Architecture.Tests;

public class ArchitectureRules
{
    private static readonly System.Reflection.Assembly CoreAssembly = typeof(IDisciplineModule).Assembly;

    private static readonly System.Reflection.Assembly[] DisciplineAssemblies =
    {
        typeof(Sport.Disciplines.FBL.FblModule).Assembly,
        typeof(Sport.Disciplines.BKB.BkbModule).Assembly,
        typeof(Sport.Disciplines.BDM.BdmModule).Assembly,
        typeof(Sport.Disciplines.VBV.VbvModule).Assembly,
        typeof(Sport.Disciplines.BOX.BoxModule).Assembly,
        typeof(Sport.Disciplines.ATH.AthModule).Assembly,
    };

    [Fact]
    public void Core_does_not_reference_any_discipline_module()
    {
        foreach (var disciplineAsm in DisciplineAssemblies)
        {
            var disciplineNs = disciplineAsm.GetName().Name!;
            var result = Types.InAssembly(CoreAssembly)
                .Should()
                .NotHaveDependencyOn(disciplineNs)
                .GetResult();

            result.IsSuccessful.Should().BeTrue($"Sport.Core must not depend on {disciplineNs}");
        }
    }

    [Fact]
    public void Each_discipline_does_not_reference_another_discipline()
    {
        foreach (var asm in DisciplineAssemblies)
        {
            var selfNs = asm.GetName().Name!;
            foreach (var other in DisciplineAssemblies)
            {
                var otherNs = other.GetName().Name!;
                if (otherNs == selfNs) continue;

                var result = Types.InAssembly(asm)
                    .Should()
                    .NotHaveDependencyOn(otherNs)
                    .GetResult();

                result.IsSuccessful.Should().BeTrue($"{selfNs} must not depend on {otherNs}");
            }
        }
    }

    [Fact]
    public void All_aggregate_root_IDs_use_Vogen_value_objects_not_raw_Guid()
    {
        var rootClassNames = new[]
        {
            "Competition", "CompetitionDiscipline",
            "Event", "Phase", "Unit", "Subunit",
            "Person", "Organisation", "Team", "Entry",
            "OfficialAssignment",
        };

        foreach (var name in rootClassNames)
        {
            var type = CoreAssembly.GetTypes().SingleOrDefault(t => t.Name == name);
            type.Should().NotBeNull($"aggregate root '{name}' must exist in Sport.Core");

            var idProp = type!.GetProperty("Id");
            idProp.Should().NotBeNull($"'{name}.Id' must be defined");
            idProp!.PropertyType.Should().NotBe(typeof(Guid),
                $"'{name}.Id' must be a Vogen typed ID, not raw Guid");
        }
    }

    private static readonly System.Reflection.Assembly ApplicationAssembly = typeof(AssemblyMarker).Assembly;

    private static readonly System.Reflection.Assembly InfrastructureAssembly =
        typeof(Sport.Infrastructure.SportDbContext).Assembly;

    [Fact]
    public void Sport_Core_does_not_reference_Sport_Infrastructure()
    {
        var result = Types.InAssembly(CoreAssembly)
            .Should().NotHaveDependencyOn("Sport.Infrastructure")
            .GetResult();
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Discipline_modules_do_not_reference_Sport_Infrastructure()
    {
        foreach (var asm in DisciplineAssemblies)
        {
            var result = Types.InAssembly(asm)
                .Should().NotHaveDependencyOn("Sport.Infrastructure")
                .GetResult();
            result.IsSuccessful.Should().BeTrue($"{asm.GetName().Name} must not depend on Sport.Infrastructure");
        }
    }

    [Fact]
    public void Sport_Infrastructure_references_Sport_Core()
    {
        var referencedAssemblies = InfrastructureAssembly
            .GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToList();
        referencedAssemblies.Should().Contain("Sport.Core",
            "Sport.Infrastructure must have a compile-time reference to Sport.Core");
    }

    [Fact]
    public void Sport_Core_does_not_reference_EntityFrameworkCore()
    {
        var result = Types.InAssembly(CoreAssembly)
            .Should().NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();
        result.IsSuccessful.Should().BeTrue("Sport.Core must remain domain-pure");
    }

    [Fact]
    public void All_EntityTypeConfigurations_are_internal_sealed()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .That().ImplementInterface(typeof(Microsoft.EntityFrameworkCore.IEntityTypeConfiguration<>))
            .Should().BeSealed()
            .And().NotBePublic()
            .GetResult();
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Sport_Application_does_not_reference_EntityFrameworkCore()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .Should().NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Sport_Application_does_not_reference_Sport_Infrastructure()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .Should().NotHaveDependencyOn("Sport.Infrastructure")
            .GetResult();
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Sport_Application_does_not_reference_any_discipline_module()
    {
        foreach (var disciplineAsm in DisciplineAssemblies)
        {
            var ns = disciplineAsm.GetName().Name!;
            var result = Types.InAssembly(ApplicationAssembly)
                .Should().NotHaveDependencyOn(ns)
                .GetResult();
            result.IsSuccessful.Should().BeTrue($"Sport.Application must not depend on {ns}");
        }
    }

    [Fact]
    public void Sport_Core_does_not_reference_Sport_Application()
    {
        var result = Types.InAssembly(CoreAssembly)
            .Should().NotHaveDependencyOn("Sport.Application")
            .GetResult();
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Sport_Core_does_not_reference_Wolverine()
    {
        var result = Types.InAssembly(CoreAssembly)
            .Should().NotHaveDependencyOn("Wolverine")
            .GetResult();
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Sport_Infrastructure_references_Sport_Application()
    {
        var refs = InfrastructureAssembly.GetReferencedAssemblies().Select(a => a.Name).ToList();
        refs.Should().Contain("Sport.Application");
    }

    [Fact]
    public void Implementations_of_Application_Abstractions_in_Infrastructure_are_sealed()
    {
        // Concrete types must be sealed.  They are declared public (not internal) because
        // Wolverine's ServiceLocationPolicy.NotAllowed requires the concrete type to be public
        // so it can generate direct constructor-injection code rather than going through the
        // service locator.  The abstraction interface is still the only thing callers depend on.
        var abstractionInterfaces = new[]
        {
            typeof(ICompetitionRepository),
            typeof(IUnitOfWork),
        };

        foreach (var iface in abstractionInterfaces)
        {
            var result = Types.InAssembly(InfrastructureAssembly)
                .That().ImplementInterface(iface)
                .Should().BeSealed()
                .GetResult();
            result.IsSuccessful.Should().BeTrue(
                $"All {iface.Name} implementations in Sport.Infrastructure must be sealed.");
        }
    }

    [Fact]
    public void DomainErrorCatalog_covers_every_domain_code_thrown_by_Sport_Core()
    {
        var observed = new HashSet<string>();
        var registry = ArchCatalogRegistry.WithFblM();
        var validDates = DateRange.Create(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 2));
        var validId = CompetitionId.From(Guid.NewGuid());
        var validCode = CompetitionCode.From("ac-1");
        var fbl = DisciplineCode.From("FBL");
        var m = GenderCode.M;
        var f = GenderCode.W;

        // I-COMP-5: blank name
        Capture(() => Competition.Create(validId, validCode, "  ", validDates,
            new[] { (fbl, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { m }) }, registry), observed);

        // I-COMP-1: no disciplines
        Capture(() => Competition.Create(validId, validCode, "N", validDates,
            Array.Empty<(DisciplineCode, IReadOnlySet<GenderCode>)>(), registry), observed);

        // I-COMP-2: unregistered discipline
        Capture(() => Competition.Create(validId, validCode, "N", validDates,
            new[] { (DisciplineCode.From("ZZZ"), (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { m }) }, registry), observed);

        // I-COMP-3: duplicate discipline
        Capture(() => Competition.Create(validId, validCode, "N", validDates,
            new[]
            {
                (fbl, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { m }),
                (fbl, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { m }),
            }, registry), observed);

        // I-COMP-4: gender not supported by discipline
        Capture(() => Competition.Create(validId, validCode, "N", validDates,
            new[] { (fbl, (IReadOnlySet<GenderCode>)new HashSet<GenderCode> { f }) }, registry), observed);

        // I-DR-1: invalid date range
        Capture(() => DateRange.Create(new DateOnly(2026, 1, 5), new DateOnly(2026, 1, 1)), observed);

        observed.Should().BeSubsetOf(DomainErrorCatalog.Map.Keys,
            "every domain code observed in Sport.Core must have a catalog entry");
    }

    private static void Capture(Action act, HashSet<string> observed)
    {
        try { act(); }
        catch (DomainException ex) { observed.Add(ex.Code); }
    }
}
