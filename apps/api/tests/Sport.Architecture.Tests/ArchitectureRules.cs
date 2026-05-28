using FluentAssertions;
using NetArchTest.Rules;
using Sport.Core.DisciplineRegistry;

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
}
