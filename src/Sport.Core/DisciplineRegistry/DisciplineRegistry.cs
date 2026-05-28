using Sport.Core.Shared;

namespace Sport.Core.DisciplineRegistry;

public sealed class DisciplineRegistry : IDisciplineRegistry
{
    private readonly Dictionary<DisciplineCode, IDisciplineModule> _modules = new();

    public void Register(IDisciplineModule module)
    {
        if (_modules.ContainsKey(module.Code))
            throw new DomainException($"Discipline '{module.Code.Value}' is already registered.");
        _modules[module.Code] = module;
    }

    public IDisciplineModule Get(DisciplineCode code) =>
        _modules.TryGetValue(code, out var m)
            ? m
            : throw new DomainException($"Discipline '{code.Value}' is not registered.");

    public bool IsRegistered(DisciplineCode code) => _modules.ContainsKey(code);

    public IReadOnlyCollection<DisciplineCode> RegisteredCodes => _modules.Keys.ToArray();

    public IReadOnlyCollection<FunctionDescriptor> CommonFunctions =>
        global::Sport.Core.DisciplineRegistry.CommonFunctions.All;
}
