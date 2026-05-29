namespace Sport.Api.ErrorHandling;

internal static class DomainErrorCatalog
{
    public static readonly IReadOnlyDictionary<string, ClientError> Map = new Dictionary<string, ClientError>
    {
        ["I-COMP-1"] = new("competition.disciplines_required",      "At least one discipline is required.",   422),
        ["I-COMP-2"] = new("competition.discipline_not_registered", "Discipline is not registered.",          422),
        ["I-COMP-3"] = new("competition.duplicate_discipline",      "Discipline appears more than once.",     422),
        ["I-COMP-4"] = new("competition.gender_not_supported",      "Gender is not supported by discipline.", 422),
        ["I-COMP-5"] = new("competition.name_required",             "Competition name is required.",          422),
        ["I-DR-1"]   = new("competition.date_range_invalid",        "End date must be on or after start.",    422),
    };

    public static ClientError? TryGet(string domainCode)
        => Map.TryGetValue(domainCode, out var error) ? error : null;
}
