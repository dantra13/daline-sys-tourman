using Sport.Core.DisciplineRegistry;
using Sport.Core.Shared;

namespace Sport.Core.Participants;

public sealed class Person
{
    public PersonId Id { get; }
    public string FamilyName { get; }
    public string? GivenName { get; }
    public GenderCode Gender { get; }
    public DateOnly? BirthDate { get; }
    public string? IFId { get; }

    private Person(PersonId id, string family, string? given, GenderCode gender, DateOnly? birth, string? ifId)
    {
        Id = id; FamilyName = family; GivenName = given; Gender = gender; BirthDate = birth; IFId = ifId;
    }

    public static Person Create(PersonId id, string familyName, string? givenName, GenderCode gender, DateOnly? birthDate, string? ifId)
    {
        if (string.IsNullOrWhiteSpace(familyName))
            throw new DomainException("Person.FamilyName is required.");
        if (familyName.Length > 50)
            throw new DomainException("Person.FamilyName must be at most 50 characters.");
        if (givenName is { Length: > 50 })
            throw new DomainException("Person.GivenName must be at most 50 characters.");
        if (ifId is { Length: > 20 })
            throw new DomainException("Person.IFId must be at most 20 characters.");
        return new Person(id, familyName, givenName, gender, birthDate, ifId);
    }
}
