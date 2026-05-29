using Sport.Core.DisciplineRegistry;
using Sport.Core.Officials;
using Sport.Core.Shared;
using Sport.Core.Structure;

namespace Sport.Core.Tests;

internal static class TestDisciplineRegistry
{
    public static IDisciplineRegistry Empty()
        => new FakeRegistry(new Dictionary<DisciplineCode, IReadOnlySet<GenderCode>>());

    public static IDisciplineRegistry With(DisciplineCode code, IEnumerable<GenderCode> genders)
        => new FakeRegistry(new Dictionary<DisciplineCode, IReadOnlySet<GenderCode>>
        {
            [code] = new HashSet<GenderCode>(genders),
        });

    private sealed class FakeRegistry : IDisciplineRegistry
    {
        private readonly Dictionary<DisciplineCode, FakeModule> _modules;

        public FakeRegistry(IReadOnlyDictionary<DisciplineCode, IReadOnlySet<GenderCode>> map)
            => _modules = map.ToDictionary(kv => kv.Key, kv => new FakeModule(kv.Key, kv.Value));

        public bool IsRegistered(DisciplineCode code) => _modules.ContainsKey(code);
        public IDisciplineModule Get(DisciplineCode code) => _modules[code];
        public IReadOnlyCollection<DisciplineCode> RegisteredCodes => _modules.Keys.ToArray();
        public IReadOnlyCollection<FunctionDescriptor> CommonFunctions => Array.Empty<FunctionDescriptor>();
    }

    private sealed class FakeModule : IDisciplineModule
    {
        public FakeModule(DisciplineCode code, IReadOnlySet<GenderCode> supported)
        {
            Code = code;
            SupportedGenders = supported;
        }

        public DisciplineCode Code { get; }
        public string DisplayName => "fake";
        public IReadOnlySet<GenderCode> SupportedGenders { get; }
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
