using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Officials;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Core.Tests.DisciplineRegistry;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddSportCore_registers_a_singleton_registry()
    {
        var services = new ServiceCollection();
        services.AddSportCore();

        var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<IDisciplineRegistry>();
        registry.RegisteredCodes.Should().BeEmpty();
    }

    [Fact]
    public void AddDisciplineModule_registers_module_into_registry()
    {
        var services = new ServiceCollection();
        services.AddSportCore();
        services.AddDisciplineModule<FakeFblModule>();

        var provider = services.BuildServiceProvider();
        var registry = provider.BuildSportRegistry();

        registry.IsRegistered(DisciplineCode.From("FBL")).Should().BeTrue();
    }

    private sealed class FakeFblModule : IDisciplineModule
    {
        public DisciplineCode Code { get; } = DisciplineCode.From("FBL");
        public string DisplayName => "Football";
        public IReadOnlySet<GenderCode> SupportedGenders { get; } = new HashSet<GenderCode> { GenderCode.M, GenderCode.W };
        public IReadOnlyCollection<EventTypeDescriptor> EventTypes => Array.Empty<EventTypeDescriptor>();
        public IPhaseCatalog PhaseCatalog => throw new NotImplementedException();
        public IUnitCodeStrategy UnitCodeStrategy => throw new NotImplementedException();
        public IReadOnlyCollection<FunctionDescriptor> Functions => Array.Empty<FunctionDescriptor>();
        public IEntryRules EntryRules => throw new NotImplementedException();
        public Result ValidateEventType(EventTypeCode type, EventModifierCode? modifier) => Result.Ok();
        public Result ValidatePhaseForEventType(EventTypeCode type, PhaseCode phase) => Result.Ok();
        public Result ValidateUnitCode(EventTypeCode type, PhaseCode phase, UnitCode code) => Result.Ok();
        public Result ValidateEntry(EntryCandidate candidate) => Result.Ok();
        public Result ValidateOfficialFunctionInScope(FunctionCode function, ScopeLevel level) => Result.Ok();
    }
}
