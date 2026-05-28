namespace Sport.Core.DisciplineRegistry;

public interface IDisciplineRegistry
{
    IDisciplineModule Get(DisciplineCode code);
    bool IsRegistered(DisciplineCode code);
    IReadOnlyCollection<DisciplineCode> RegisteredCodes { get; }
    IReadOnlyCollection<FunctionDescriptor> CommonFunctions { get; }
}
