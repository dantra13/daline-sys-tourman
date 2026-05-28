using Sport.Core.DisciplineRegistry;
using Sport.Core.Officials;
using Sport.Core.Participants;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Infrastructure.Tests.TestHelpers;

public sealed class FakeRegistry : IDisciplineRegistry
{
    public HashSet<DisciplineCode> SupportedCodes { get; } = new();
    public Dictionary<DisciplineCode, IReadOnlySet<GenderCode>> GendersByCode { get; } = new();

    public IDisciplineModule Get(DisciplineCode code) =>
        new FakeModule(code, GendersByCode[code]);
    public bool IsRegistered(DisciplineCode code) => SupportedCodes.Contains(code);
    public IReadOnlyCollection<DisciplineCode> RegisteredCodes => SupportedCodes.ToArray();
    public IReadOnlyCollection<FunctionDescriptor> CommonFunctions => Array.Empty<FunctionDescriptor>();

    private sealed class FakeModule(DisciplineCode code, IReadOnlySet<GenderCode> genders) : IDisciplineModule
    {
        public DisciplineCode Code { get; } = code;
        public string DisplayName => "fake";
        public IReadOnlySet<GenderCode> SupportedGenders { get; } = genders;
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
