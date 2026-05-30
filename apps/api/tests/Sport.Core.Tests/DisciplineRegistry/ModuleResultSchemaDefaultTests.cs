using FluentAssertions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Officials;
using Sport.Core.Results;
using Sport.Core.Structure;

namespace Sport.Core.Tests.DisciplineRegistry;

public class ModuleResultSchemaDefaultTests
{
    [Fact]
    public void Module_without_override_exposes_permissive_default_schema()
    {
        IDisciplineModule module = new BareModule();
        module.ResultSchema.Should().BeOfType<DefaultResultSchema>();
        module.ResultSchema.OutcomeModeFor(EventTypeCode.From("ANY")).Should().Be(OutcomeMode.HeadToHead);
    }

    private sealed class BareModule : IDisciplineModule
    {
        public DisciplineCode Code => DisciplineCode.From("ZZZ");
        public string DisplayName => "Bare";
        public IReadOnlySet<GenderCode> SupportedGenders => new HashSet<GenderCode> { GenderCode.M };
        public IReadOnlyCollection<EventTypeDescriptor> EventTypes => Array.Empty<EventTypeDescriptor>();
        public IPhaseCatalog PhaseCatalog => null!;
        public IUnitCodeStrategy UnitCodeStrategy => null!;
        public IReadOnlyCollection<FunctionDescriptor> Functions => Array.Empty<FunctionDescriptor>();
        public IEntryRules EntryRules => null!;
        public Sport.Core.Shared.Result ValidateEventType(EventTypeCode type, EventModifierCode? modifier) => Sport.Core.Shared.Result.Ok();
        public Sport.Core.Shared.Result ValidatePhaseForEventType(EventTypeCode type, PhaseCode phase) => Sport.Core.Shared.Result.Ok();
        public Sport.Core.Shared.Result ValidateUnitCode(EventTypeCode type, PhaseCode phase, UnitCode code) => Sport.Core.Shared.Result.Ok();
        public Sport.Core.Shared.Result ValidateEntry(EntryCandidate candidate) => Sport.Core.Shared.Result.Ok();
        public Sport.Core.Shared.Result ValidateOfficialFunctionInScope(FunctionCode function, ScopeLevel level) => Sport.Core.Shared.Result.Ok();
    }
}
