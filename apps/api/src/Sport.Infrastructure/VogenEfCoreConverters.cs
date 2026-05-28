using Sport.Core.Competitions;
using Sport.Core.DisciplineRegistry;
using Sport.Core.Officials;
using Sport.Core.Participants;
using Sport.Core.Structure;
using Vogen;

namespace Sport.Infrastructure;

[EfCoreConverter<CompetitionId>]
[EfCoreConverter<CompetitionDisciplineId>]
[EfCoreConverter<CompetitionCode>]
[EfCoreConverter<EventId>]
[EfCoreConverter<PhaseId>]
[EfCoreConverter<UnitId>]
[EfCoreConverter<SubunitId>]
[EfCoreConverter<Rsc>]
[EfCoreConverter<EventTypeCode>]
[EfCoreConverter<EventModifierCode>]
[EfCoreConverter<PhaseCode>]
[EfCoreConverter<UnitCode>]
[EfCoreConverter<SubunitCode>]
[EfCoreConverter<PersonId>]
[EfCoreConverter<OrganisationId>]
[EfCoreConverter<TeamId>]
[EfCoreConverter<EntryId>]
[EfCoreConverter<OrganisationCode>]
[EfCoreConverter<TeamCode>]
[EfCoreConverter<Bib>]
[EfCoreConverter<OfficialAssignmentId>]
[EfCoreConverter<FunctionCode>]
[EfCoreConverter<DisciplineCode>]
internal partial class VogenEfCoreConverters;
