using FluentAssertions;
using NetArchTest.Rules;
using Sport.Core.Results;

namespace Sport.Architecture.Tests;

public class ResultsLayerArchitectureTests
{
    private static readonly System.Reflection.Assembly CoreAssembly =
        typeof(UnitResultDocument).Assembly;

    private static readonly string[] DisciplineNamespaces =
    {
        typeof(Sport.Disciplines.FBL.FblModule).Assembly.GetName().Name!,
        typeof(Sport.Disciplines.BKB.BkbModule).Assembly.GetName().Name!,
        typeof(Sport.Disciplines.BDM.BdmModule).Assembly.GetName().Name!,
        typeof(Sport.Disciplines.VBV.VbvModule).Assembly.GetName().Name!,
        typeof(Sport.Disciplines.BOX.BoxModule).Assembly.GetName().Name!,
        typeof(Sport.Disciplines.ATH.AthModule).Assembly.GetName().Name!,
        typeof(Sport.Disciplines.JUD.JudModule).Assembly.GetName().Name!,
    };

    [Fact]
    public void Results_namespace_does_not_depend_on_any_discipline()
    {
        foreach (var disciplineNs in DisciplineNamespaces)
        {
            var result = Types.InAssembly(CoreAssembly)
                .That().ResideInNamespace("Sport.Core.Results")
                .Should().NotHaveDependencyOn(disciplineNs)
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                $"Sport.Core.Results must not depend on {disciplineNs}");
        }
    }
}
