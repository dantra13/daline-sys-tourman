# ODF GEN General Messages Interface: Document Control, Pages 411-420

Source: `C:\Users\mella\WebstormProjects\sportivo\docs\references\odf\ODF_GEN_R-OWG2026-GEN.pdf`, pages 411-420.

This reference extracts section `4 Document Control` from the OWG 2026 General Messages Interface Document (`OWG2026-GEN-4.6`, APP, 10 December 2025). It is intended to track version-to-version changes in the general ODF interface and to explain drift between the PDF dictionaries and XSD/schema artifacts.

## Version History

| Version | Date | Comments |
|---|---|---|
| `2018-0.1` | 4 May 2015 | First Version for PyeongChang 2018 |
| `2018-0.2` | 9 Jul 2015 | New Federation Ranking included and other updates |
| `2018-0.3` | 16 Jul 2015 | Editorial corrections |
| `2018-0.4` | 9 Sep 2015 | Change Requests applied |
| `2018-0.5` | 7 Oct 2015 | Change Request Applied |
| `2018-0.6` | 6 Nov 2015 | Change Request Applied |
| `2018-0.7` | 24 Mar 2016 | Change requests and minor editing |
| `2018-0.8` | 19 May 2016 | Minor corrections |
| `2018-0.9` | 24 Jun 2016 | CRs, Minor corrections/typographical errors |
| `2018-1.0` | 22 Sep 2016 | Minor corrections |
| `2018-1.1` | 10 Nov 2016 | Typographical correction and minor improvement |
| `2018-1.2` | 22 Dec 2016 | Typographical corrections and CRs |
| `2018-1.3` | 23 Feb 2017 | Typographical corrections and change requests |
| `2018-1.4` | 20 Apr 2017 | Typographical corrections and change requests |
| `2018-1.5` | 25 May 2017 | Change Requests |
| `2018-1.6` | 2 Oct 2017 | Change Requests |
| `2018-1.7` | 4 Dec 2017 | Change Request |
| `2020-1.0` | 1 Aug 2018 | Change Requests |
| `2020-1.1` | 5 Dec 2018 | Change Requests and defect resolution |
| `2020-1.2` | 18 Apr 2019 | Change Requests and defect resolution |
| `2020-1.3` | 30 May 2019 | Change request and clarifications. |
| `2020-1.4` | 14 Aug 2019 | Change request and clarifications. |
| `2020-1.5` | 11 Nov 2019 | Change request and clarifications. |
| `2020-1.6` | 10 Dec 2019 | Updated with CRs |
| `2020-1.7` | 6 Feb 2020 | Updated with CR |
| `2020-1.8` | 14 Feb 2020 | Updated with typographical corrections |
| `2020-2.0` | 15 May 2020 | Updated with CRs |
| `2020-2.1` | 22 Jul 2020 | Updated with CRs |
| `2020-2.2` | 4 Sep 2020 | Restructure |
| `2020-2.3` | 27 Nov 2020 | Updated with CRs |
| `2022-2.4` | 25 Jun 2022 | First version for Beijing 2022 |
| `2022-2.5` | 10 Sep 2021 | Updated with CRs and clarifications |
| `2022-2.6` | 12 Nov 2021 | Updated with CRs |
| `2024-3.0` | 10 Dec 2021 | First version for Paris 2024 |
| `2024-3.1` | 1 Jul 2022 | Change requests |
| `2024-3.2` | 14 Oct 2022 | Change requests |
| `2024-3.3` | 9 Dec 2022 | Change request |
| `2024-3.4` | 5 May 2023 | Corrections and CR |
| `2024-3.5` | 9 Jun 2023 | Updated |
| `2024-3.6` | 3 Nov 2023 | CR026768 |
| `2024-3.7` | 23 Feb 2024 | CR026994 |
| `2026-4.0` | 17 May 2024 | 1st Draft version for Milano Cortina |
| `2026-4.1` | 2 August 2024 | Common changes and updates for Milano Cortina |
| `2026-4.2` | 8 October 2024 | Typographical correction and minor improvement |
| `2026-4.3` | 31 January 2025 | CHG0033133, CHG0032327, CHG0032327 and editorial updates |
| `2026-4.4` | 11 April 2025 | CHG0034175, corrections |
| `2026-4.5` | 31 July 2025 | CHG0035534, corrections-adjustments |
| `2026-4.6` | 10 December 2025 | Editorial updates, CHG0039762 |

## Detailed Change Log

### `2018-0.1` (SFR)
- First Version

### `2018-0.2` (SFR)
- Updated with changes from Rio GL document
- Updated samples to use new code system (CR7454)
- Added ExtendedInfos extensions in DT_SCHEDULE for use when generated from the competition schedule application.

### `2018-0.3` (SFR)
- Clarified the term "Full RSC" to use it consistently.
- Some minor typographical errors
- Added some winter sport samples
- Corrected field sizes in the codes message

### `2018-0.4` (SFR)
- Applied Change Results
- CR7429 - Add date in DT_MEDALLISTS message
- CR7452 - Rename stats elements in DT_RESULT (and therefore DT_RESULT_ANALYSIS and DT_ESL)
- CR7455 - ExtendedResults in DT_CUMULATIVE_RESULT
- CR7456 - Add support for teams of Teams in DT_PARTIC_TEAMS
- CR7457 - Add ResultItems to DT_RESULT message (and therefore DT_RESULT_ANALYSIS and DT_ESL)

### `2018-0.5` (SFR)
- LIVE Status added to DT_BRACKETS message
- Correct error in Disciple Medallists to be clear
- CR8126 - Add statistics in DT_CURRENT

### `2018-0.6` (SFR)
- CR8254 - Add discipline in DT_PIC
- Includes changes made in Rio documentation

### `2018-0.7` (SFA)
- For consistency, TeamName in the Competitor/Description is changed to always mandatory (though Description is not). Previously different depending on the message.
- Updated text in Order attribute related to sending 1 if only one exist.
- CR8928 - DT_RESULT/ANALYSIS/ESL add 'Attendance' as attribute at ExtendedInfos/VenueDescription
- DT_BRACKETS add Bib at Competitor and Competitor/Composition/Athlete
- DT_RANKING add 'Diff' as an attribute at Result and 'Bib' as an attribute at Competitor and at Competitor/Composition/Athlete
- DT_PARTIC and DT_PARTIC_TEAM add 'Substitute' and 'Status' at Discipline/RegisteredEvent
- CR8930 - Consistent use of DocumentSubtype and DocumentSubcode (add DocumentSubcode in Phase, Cumulative and pool messages.)
- CR8933 - Applied default sort order for DT_CUMULATIVE_RESULTS
- CR8934 - Add START_LIST and IRMs to brackets and remove LIVE ResultStatus
- CR8936 - Add H1 Headings to DT_CODES message.
- CR8938 - Normalising the ExtendedInfos for DT_PDF
- CR9036 - Change <Competition> element to cardinality (0,1) to allow for message invalidation.
- CR9360 - Play by Play message improvements (in play by play and current messages)
- CR9361 - Communication message improvements
- CR9941 - Add Result attribute at CompetitorPlace in DT_BRACKETS
- CR9942 - Add home/away indicator in Pool Standings

### `2018-0.8` (SFA)
- Clarify that only the ENG description of the unit is expected in the schedule messages.

### `2018-0.9` (SFA)
- Correct typographical errors in samples
- In DT_SCHEDULE updated to support SC @StartText
- CR10294 - DT_ALERT: Add two new DocumentSubtypes of NEWS and RESULTS (2.2.19.2)
- CR10246 - Add TVTeamName to Team participants message (DT_PARTIC_TEAMS).
- CR11930 - Remove DocumentSubcode from DT_SCHEDULE & DT_SCHEDULE_UPDATE

### `2018-1.0` (APP)
- DT_RECORD: Clarify the order of the data in the message. Also clarify that <RecordData> can be sent for not established records where a standard applies.

### `2018-1.1` (APP)
- DT_FED_RANKING: Rankings /Ranking /ExtRanking /ExtRank should have cardinality of (0,N)
- DT_BRACKETS: Provide more information on when the time should be included at Bracket /BracketItems /BracketItem.

### `2018-1.2` (APP)
- Correct typo. Add TVTeamName in DT_PARTIC_TEAMS which was accidentally removed.
- Correct type. Add extension in DT_SCHEDULE for status and version which was accidentally removed.
- DT_CURRENT: Update to include information on how to use the clock data.
- DT_IMAGE: CR14627 - Add Result Element to include competitors in the message.

### `2018-1.3` (APP)
- DT_PARTIC: Clarify that all applicable participants are included regardless of status [CR14576]
- DT_RESULT: Add Rank, RankEqual and SortOrder to StatisticItem (athlete and competitor) [CR14580]
- DT_RESULT: Add DocumentSubcode in the Header [CR14628]
- DT_RESULT: Remove StartListMod in the ODF Header [CR14579]
- DT_RESULT: In ExtendedInfos change StartDate and EndDate to be actual only, do not include until unit starts/ends [CR14578]
- DT_PLAY_BY_PLAY/DT_CURRENT: Modify the TimeStamp to be in DateTime format. [CR14577]
- DT_GPS_DATA: Message Removed [CR14586]
- DT_POOL_STANDING: Update the description Result/Ratio to "Ratio value, see sports documents for more information" thus making it more generic and flexible.
- DT_PIC: Added 'HEADSHOT' as possible DocumentSubtype. [CR14630]
- Correct samples of team code where incorrect(typo) [ATHM4X400M--ESP01].
- Other minor typographical errors without changing the meaning

### `2018-1.4` (APP)
- DT_RESULT: Add duration in ExtendedInfos [CR14578]
- DT_SCHEDULE: Add attributes PreviousWLT and PreviousUnit at element Unit/StartList/Start

### `2018-1.5` (APP)
- DT_SCHEDULE: Description of use for the Order@Unit attribute updated to be more clear when special ordering is required
- DT_CONFIG: Triggering updated to clarify that new version of DT_RESULT as soon as DT_CONFIG changes.
- DT_IMAGE: Type@Result/Competitor changed to Optional
- DT_CURRENT: DocumentSubtype added to support distinction when DT_CURRENT is used for more than one purpose.
- DT_RESULT: Triggering: ResultStatus description updated to provide more detail.
- DT_BIO_PAR/DT_BIO_PAR_IMP: Add flag as an extension to indicate that the athlete participated in the Youth Olympic Games.

### `2018-1.6` (APP)
- CR08929 Add Add new message for Medal Presenters (DT_PRESENTER)
- CR15039 Add DT_PARTIC_NAME message (for use after PyeongChang)
- CR15219 Add passport names to DT_PARTIC message (for use after PyeongChang)
- CR15263 Add support uniform images in DT_IMAGE (for use after PyeongChang)

### `2018-1.7` (APP)
- CR15803: Update DT_SCHEDULE for non-competition items
- DT_SCHEDULE: Minor editorial.

### `2020-1.0` (APP)
- CR16078: Add scoreboard names in DT_PARTIC_NAME message.
- CR16537: Add Progress element in ExtendedInfos in DT_PHASE_RESULT, DT_CUMULATIVE_RESULT, DT_BRACKET, DT_RANKING, DT_STATS, DT_POOL_STANDING.
- CR16538: Align event order to the IF Event presentation order in DT_MEDALLISTS_DISCIPLINE (see sort order)
- CR16540: Add DT_MEDALS to be sent at discipline level as well as the existing overall level.
- CR16541: Change Document/Title to free text in DT_NEWS and DT_BCK messages.
- CR16626: Increase triggering in DT_MEDALLIST to include UNOFFICIAL.
- CR16627: Increase size of DocumentSubtype in DT_PDF to allow use of team codes rather than NOC codes for statistics.
- CR16628: In DT_BRACKETS add attributes to remove need for extensions and simplify processing. Clarify previous unit.
- CR16671: Add TV family name into the DT_PARTIC and DT_PARTIC_NAME messages.
- DT_CODES: Correct typo in RECORD message

### `2020-1.1` (APP)
- CR16616: Change unit medal type in DT_SCHEDULE/DT_SCHEDULE_UPDATE
- CR16692: Add support for teams of teams in some messages.
- CR16716: Changes in DT_ALERT to add SERVICE message, change DocumentCode and update triggering.
- CR16833: Update DT_SCHEDULE to clarify and allow UNSCHEDULED units in message.
- CR16910: Updates in DT_BIO_PAR for data related to birth and residence.
- CR16914: Updates in DT_WEATHER to use venue level and adding extensions
- CR16920: Updates in DT_CODES to add tables and fields to the message.
- CR16928: Add more addributes in results to remove some common extensions.
- CR17019: Increase the field size in some elements in athlete and NOC biographies.
- DT_PDF: Update ExtendedInfos EI/REPORTTITLE to be clear.
- DT_PDF: Fixed defect to add Source to the header, was missing in error.
- Defect correction in the identifier for news, background, transport and alerts. Change from numeric to string. Applies in DocumentSubcode and Parent within the message.

### `2020-1.2` (APP)
- CR16542: Add DT_FLAGBEARERS message
- CR17269: Change athlete biographies to add field 'Milestones'
- CR17284: Add UnitNum in DT_PLAY_BY_PLAY
- CR17391: Clarify DT_MEDALS (data provided) and DT_MEDALLIST_DISCIPLINE (sort order)
- CR17421: Update DT_PRESENTER definition to manage initial list
- DT_SCHEDULE_UPDATE: Reword triggering to improve clarity
- DT_BCK: Correct typographical error in sample in sample

### `2020-1.3` (APP)
- CR16640: Add ODF Version in Competition Element
- CR17409: Add Short Description in DT_CODES for NOC table
- CR17521: Add more detail in Team of Teams in applicable messages
- Editorial improvements and typographical corrections without changing the intent.

### `2020-1.4` (APP)
- CR17739: Change Name and TVTeamName to mandatory in DT_PARTIC_TEAMS
- CR17808: Add Competititon/Officials and Competititon/Stats/Competitor/Coaches in DT_STATS
- CR17809: Change Participant/OlympicSolidarity to disallow N
- CR17826: Add Competition/Session/Medal in DT_SCHEDULE/_UPDATE
- CR17827: Add Competititon/StatsItems and Competititon/Result/Competitor/Coaches/Coach/ExtCoach in DT_RESULT (and associated DT_RESULT_ANALYSIS, DT_ESL)
- CR18056: Update ResultsItems in DT_RESULT, DT_CUMULATIVE_RESULT, DT_ESL & DT_CUMULATIVE_RESULT to include the same attributes as in Result & ExtendedResult
- DT_CODES: Add definition for EVENT_CLASS and DISCIPLINE_CLASS

### `2020-1.5` (APP)
- CR18316: Add option for .png in DT_PIC/HEADSHOT
- CR18355: Add ResultStatus START_LIST in DT_POOL_STANDING
- CR18395: Increase size of SessionCode in DT_SCHEDULE/_UPDATE
- CR18396: Add REPORT_STATUS in ExtendedInfos in DT_PDF
- DT_SCHEDULE: Clarify SessionCode in the case of interupted units.
- Correct typographical errors in samples

### `2020-1.6` (APP)
- CR018560: DT_MEDALLISTS: Add clarification in triggering
- CR018565: DT_PARTIC: Remove dash from weight as an option
- CR018622: DT_MEDALS Add clarification and remove 0s
- Clarification in DT_PDF header without changing the intent.

### `2020-1.7` (APP)
- Correct minor typographical errors.
- DT_PRESENTER: Update the length of Role (to 45) and PresenterName (to 32) [CR18702]
- DT_MEDALLIST_DISCIPLINE: Correct the error in cardinality of ExtendedInfos /ExtendedInfo. [188151]

### `2020-1.8` (APP)
- DT_VEN_COND: Correct typographical error in message structure for Precipitaion and Pressure attributes (attributes were correct in Message Values and schema).
- DT_BCK: Correct the error in DocumentCode. Send RSC at Discipline Level.

### `2020-2.0` (APP)
- DT_SCHEDULE: Add BYE at Competition /Unit /StartList /Start /Competitor for flexibility [CR019493]
- DT_PARTIC_TEAMS: Add Team/TeamType and Team/ShortName [CR019497]
- DT_RESULT: Add BYE at Result/Competitor for flexibility [CR019493]
- DT_CUMULATIVE_RESULT: Add ResultStatus START_LIST [CR019493]
- DT_MEDALLISTS_DISCIPLINE: Add extensions for consistency with DT_MEDALLIST [CR019495]
- DT_FLAG_BEARERS: Update message to support multiple flagbearers [CR019572]
- DT_BIO_NOC: Add flexibility for multiple flagbearers [CR019246]
- DT_CODES: Add missing tables to message [CR019492]
- DT_ALERT: Add Document/Code to allow for translations in standard alerts [CR019494]
- DT_WEA_ALERT: Add the cardinality which was missing in the elements under Place/Alert (typographical error)
- DT_PDF: Add flexibility in ResultStatus [CR019493]

### `2020-2.1` (APP)
- DT_VEN_COND: Clarify format at Venue /DateTime /Conditions /Humidity (##0)
- DT_VEN_COND: Clarify format at Venue /DateTime /Conditions /Wind_Degree (##0)
- DT_CODES: Update Location Code set table to include ShortDescription [CR19968] (applicable from Beijing 2022)
- DT_PING: Add message [CR19969]

### `2020-2.2` (APP)
- Document restructured to add responsibilities table and merge venue and central messages
- DT_SCHEDULE: Update message description to include Y and S units in applying CR020215
- DT_PDF: Update ResultStatus (adding START_LIST) in the header to match OVR implementation

### `2020-2.3` (APP)
- DT_CODES: Add Group (Partic) to the DISCIPLINE_FUNCTION message [CR020722]
- DT_PRESENTER: Update message to align to updated IOC process [CR020742]

### `2022-2.4` (APP)
- DT_LOCAL_ON: Update DocumentCode in header (clarity, no data change)
- DT_LOCAL_OFF: Update DocumentCode in header (clarity, no data change)
- DT_KA: Update DocumentCode in header (clarity, no data change)
- DT_PING: Update DocumentCode in header (clarity, no data change)
- DT_ALERT: Update Document/Code and Document/Message/- for clarity, only impacted if DocumentSubtype = RESULTS
- DT_BCK_IMP: Update Document/FileName to S(20) to increase flexibility [CR021625]
- DT_PARTIC_TEAMS: Change Team/ShortName and Team/TeamType to M [CR019497]
- Other editorial improvements to add clarity the document without changing any messages.

### `2022-2.5` (APP)
- DT_POOL_STANDING: Clarify Result /Competitor /Opponent /Pos - no change in information.
- DT_ACHIEVEMENT: Message added. CR023194
- DT_TV_TRACKING: Clarification in the Description.

### `2022-2.6` (APP)
- DT_SCHEDULE: Clarification at Unit [CR024248]
- DT_PARTIC: Update to add DocumentSubtype for HISTORICAL messages [CR024157]
- DT_PARTIC: Update to add DocumentSubtype for HISTORICAL messages [CR024157]
- DT_PARTIC: Update to add DocumentSubtype for HISTORICAL messages [CR024157]
- DT_MEDALLISTS: Update triggering [CR024155]
- DT_PDF: Update DocumentSubcode for C49 [CR024156]
- DT_SCHEDULE: Update cardinality of Unit /VenueDescription to 0:1 [HPQC199360]
- Change / document field length in bio messages [CR024159]
- DT_POOL_STANDING: Clarify Result /Competitor /Opponent /Pos (editorial)
- DT_BRACKETS: Clarify Bracket /BracketItems /BracketItem /Position (editorial)
- DT_BIO_PAR: Update size in the following:
- ParticipantBiography /Language/GInterest /Family
- ParticipantBiography /Language/GInterest /Start
- DT_BIO_TEA: Update size in the following:
- TeamBiography /Language /CHighlights /Highlights
- TeamBiography /Language /GInterest /Music
- TeamBiography /Language /GInterest /Choreographer
- DT_BIO_NOC: Add size in the following:
- Organisation /Language /Anthem /Title
- Organisation /Language /Anthem /Composer
- Organisation /Language /Membership /OfficialNocName
- Organisation /Language /Membership /CountriesIncluded
- Organisation/Language /Officials /NOCPresident
- Organisation /Language /Officials /NOCGenSecretary
- Organisation /Language /Officials /IOCMembers
- Organisation /Language /Officials /IOCExecBoard
- Organisation/Language /Participation /FirstOGAppearance
- Organisation /Language /Participation /NumOGAppearance

### `2024-3.0` (SFA)
- DT_SCHEDULE: Update Medal and add FOP at Session [CR023122]
- DT_PARTIC: Add Discipline/RegisteredEvent/EntryStatus [CR021163]
- DT_BRACKETS: Update to add @ResultType at CompetitorPlace [CR023121]
- DT_CODES: Update message to separate by language [CR023122]
- DT_COMMUNICATION: Update to send at any level [CR024349]
- DT_MEDALLISTS_DISCIPLINE: Clarify sort order [CR024154]
- DT_MEDALLISTS: Clarify triggering [editorial to remove inconsistency

### `2024-3.1` (APP)
- DT_MEDALLIST_DISCIPLINE: Update triggering [CR024807]
- DT_MEDALS: Update triggering [CR24807]
- DT_PDF: Update ResultStatus [CR024870]

### `2024-3.2` (APP)
- DT_PHASE_RESULT: Update Result and ExtendedResult attributes to follow DT_RESULT [CR024957]
- DT_PIC: Update DocumentSubtype to add HORSE [CR024987]
- DT_COMMUNICATION: Add Communication /Protest /Procedure and Communication /Protest /Informed and update attribute names Initiator and Respondent (from Protestor and Protestee) [CR025100]

### `2024-3.3` (APP)
- ResultStatus updated to include PROVISIONAL [CR025172]. Affects Results, Results Analysis, Play by Play, Phase Results, Cumulative Result, Pool Standings, Brackets, Stats, Event Ranking, Medallists, PDF.
- Add a new DocumentSubtype in DT_PARTIC, DT_PARTIC_TEAM and DT_SCHEDULE to indicate it is a full message after the venue has begun sending _UPDATEs [CR025269]

### `2024-3.4` (APP)
- DT_SCHEDULE: Update Unit/ItemName/Value [clarification to align with current implementation CR025554]
- DT_RESULT: Update Periods/Period/HomeScore to O [correcting typo to now match schema]
- DT_RESULT: Update Periods/Period/AwayScore to O [correcting typo to now match schema]
- DT_POOL_STANDING: Update Result /Competitor /Opponent /Date to O [correcting typo to now match schema]
- DT_BIO_HOR: Add SireDam at HorseBiography [CR025445]
- DT_ALERT: Update throughout for RESULTS alerts [CR025171]
- DT_PDF: Clarify ResultStatus [CR025566]
- DT_CODES: Update to add language sort order in NOC/NPC and Discipline [CR025531]
- DT_VEN_COND: Update throughout for clarity and make forecast only [CR025662]

### `2024-3.5` (APP)
- Typographical corrections/improvements, no impact in messages

### `2024-3.6` (APP)
- DT_AUDIO: Message added [CR026768]

### `2024-3.7` (APP)
- DT_CODES: Update cardinality at CodeSet/Language
- DT_MEDALLISTS: Update triggering
- DT_MEDALLISTS_DISCIPLINE: Update triggering
- DT_MEDALS: Update triggering

### `2026-4.0` (SFR)
- Editorial corrections/improvements, new patterns applied to values
- For all messages for the element Competition the attributes Gen and Codes are set to M.
- Sport attribute in element Competition has been changed to S(35)
- ModificatorIndicator deleted in all applicable messages
- Messages Table (section 2) updated
- DT_SCHEDULE/DT_SCHEDULE_UPDATE: Competition /Unit /StartList /Start @PreviousValue added
- DT_PARTIC/DT_PARTIC_UPDATE: new structure applied
- Competition /Participant Status set to M TVFamilyName changed to S(18) PSCBName, PSCBShortName, PSCBLongName added MainFunctionId set to O
- DT_PARTIC_TEAMS/DT_PARTIC_TEAMS_UPDATE: new structure applied
- Competition /Team @Status added, PSCBName, PSCBShortName, PSCBLongName added
- DT_PARTIC_HORSES/ DT_PARTIC_HORSES_UPDATE:
- Competition /Horse @Status added
- DT_ENTRIES/DT_ENTRIES_TEAMS added
- DT_RESULT:
- Competition /ExtendedInfos /ExtendedInfo /Competitor deleted
- Competition /ExtendedInfos /PreviousResults and subelements deleted
- DT_RESULT_ANALYSIS:
- Competition /ExtendedInfos /ExtendedInfo /Competitor deleted
- Competition /ExtendedInfos /PreviousResults and subelements deleted
- DT_PLAY_BY_PLAY:
- Competition /ImageData deleted
- DT_PHASE_RESULT:
- Competition /Result @WLT added
- Competition /Result /ResultItems /ResultItem and subelements added
- Competition /Result /Competitor /EventUnitEntry added
- Competition /Result /Competitor /Composition /Athlete @StartOrder and StartSortOrder added
- Competition /Result /Competitor /Composition /Athlete /EventUnitEntry added
- Competition /Result /Competitor /Composition /Team /EventUnitEntry added
- Competition /Result /Competitor /Composition /Team /Composition /Athlete StartOrder and StartSortOrder added
- Competition /Result /Competitor /Composition /Team /Composition /Athlete /EventUnitEntry added
- DT_CUMULATIVE:
- Competition /Result Unchecked, WLT, StartOrder, StartSortOrder added
- Competition /Result /Competitor /EventUnitEntry added
- Competition /Result /Competitor /Composition /Athlete StartOrder and StartSortOrder added
- Competition /Result /Competitor /Composition /Athlete /EventUnitEntry added
- Competition /Result /Competitor /Composition /Athlete /ExtendedResults /ExtendedResult Value2, Diff, Speed, Move, Pty, Discard, Arrive, Unchecked added
- Competition /Result /Competitor /Composition /Team /EventUnitEntry added
- Competition /Result /Competitor /Composition /Team /ExtendedResults /ExtendedResult Speed, Move, Pty, Discard, Arrive, Unchecked added
- Competition /Result /Competitor /Composition /Team /Composition /Athlete StartOrder and StartSortOrder added
- Competition /Result /Competitor /Composition /Team /Composition /Athlete /EventUnitEntry added
- DT_IMAGE:
- DocumentSubtype=COURSEMAP added
- DT_PRESSPHOTOFINISH_LK:
- Competition /ExtendedInfos and subelements deleted
- DT_POOL_STANDING:
- Competition /Result /Competitor /Opponent @TimeStamp added
- DT_BRACKETS:
- Competition /Bracket /BracketItems /BracketItem @TimeStamp added
- DT_STATS:
- DocumentSubtype and ResultStatus header values updated.
- DT_RECORDS:
- Competition /Record /RecordType /RecordData @TimeStamp and Reinstated added
- DT_RANKING:
- Competition /Result /Competitor: Value "NOC" deleted
- DT_MEDALLISTS:
- Trigger and Frequency text updated.
- DT_FED_RANKING: structure updated.
- DT_LOCAL_ON/DT_LOCAL_OFF/DT_KA/DT_PING:
- DocumentCode value format updated.
- DT_BCK: Trigger text updated
- DT_BIO_PAR:
- Header Values updated (DocumentCode format changed, DocumentSubtype added).Trigger text updated.
- DT_BIO_TEAM, DT_BIO_NOC, DT_BIO_HOR:
- Trigger text updated.
- DT_ACHIEVEMENT:
- Competition/Sport added for consistency in the structure.
- DT_ALERT:
- DocumentSubtype=SERVICE removed, Trigger text updated.
- DT_NEWS: Trigger text updated.
- DT_TRS and DT_TRS_IMP: removed
- DT_PIC: Trigger text updated.
- DT_AUDIO: Trigger text updated
- DT_CODES: Structure and Code Sets updated

### `2026-4.1` (SFR)
- DT_ENTRIES: New structure applied
- DT_ENTRIES_TEAMS: Deleted
- Guide Element: introduced across all applicable message types, removing the Guide information in extensions and in Athlete /Description element.
- PhotoFinish attribute introduced under the elements: Competition /Result and Competition /Result /ResultItems /ResultItem /Result applicable to the DT_RESULT, DT_RESULT_ANALYSIS, DT_CURRENT and DT_BRACKETS message types.
- DT_SCHEDULE: Description, Structure updated
- DT_RECORD: Competition Sport attribute changed to Mandatory.
- DT_IMAGE: Competition /Image /Result attributes ResultType and IRM added.
- DT_FED_RANKING: Structure updated
- DT_AUDIO: Description, Structure updated

### `2026-4.2` (SFR)
- DT_SCHEDULE: Message description updated; Competition /Unit /Code Description updated
- DT_ENTRIES: Trigger and Frequency updated
- DT_CURRENT: Editorial updates
- DT_BCK: Editorial updates
- DT_BIO_PAR and DT_BIO_PAR_IMP: Structures and Message values separated
- DT_BIO_TEA and DT_BIO_TEA_IMP: Structures and Message values separated
- DT_BIO_NOC and DT_BIO_NOC_IMP: Structures and Message values separated
- DT_BIO_HOR and DT_BIO_HOR_IMP: Structures and Message values separated
- DT_ALERT, DT_NEWS: Editorial updates
- DT_CODES: DISCIPLINE_CLASS and EVENT_CLASS code sets definition updated

### `2026-4.3` (SFA)
- Editorial Updates
- DT_BIO_PAR: DocumentCode update.
- DT_BIO_TEA_IMP: Competition/ TeamBiography attribute Current value updated.
- DT_ESL: Message Structure updated to match DT_RESULT (editorial)
- DT_IMAGE: Header values updated to allow DocumentCode at Discipline Level
- DT_KA: Message Headers: DocumentCode updated for the central systems triggered KA messages
- DT_MEDALLISTS_DAY: Structure corrected to match message values
- DT_NEWS: Competition /Document Attributes Category, Item and Unit values updated
- DT_PARTIC_TEAMS: Competition /Team /Discipline Correction of the Element cardinality in the Message Values table to match the Message Structure definition.
- DT_PARTIC_TEAMS: Competition /Team /Code description clarification to support historical teams
- DT_RECORD: Structure extended to support Teams of a Team (CHG0033133),
- DT_RECORD: Competition /Record /RecordType /RecordData /Competitor /Composition /Athlete cardinality updated to (0,N)
- DT_SCHEDULE: Competition /Unit /ItemName /Value: All possible Unit types added, ENG Description should be used.
- DT_SCHEDULE: Competition /Unit /HideUnitNum added in the Message values (editorial correction)
- DT_SCHEDULE: Competition /Unit /MediaAccess added
- DT_SCHEDULE: clarifications added in for the management of early stages of the competition and for non-sport activities.
- DT_SCHEDULE: Header Values, DocumentSubtype PRE added as applicable value
- DT_VEN_COND: Competition /Venue /Conditions cardinality (0,1) added, Code expanded to support multiple Weather Points
- DT_VEN_COND: Competition /DateTime /Conditions /Precipitation removed (CHG0032327)
- DT_VEN_COND: Competition /DateTime /Conditions /Pressure removed (CHG0032327)
- DT_WEATHER: Competition /Weather /Conditions /Wind Value and Description updated
- DT_WEA_ALERT: Clarifications in the triggering and Alert/ Code
- DT_WEA_ALERT: Header Values: Document code and DocumentSubcode updated, DocumentSubtype deleted.
- DT_CURRENT: Competition /Clock (0,1) Time attribute Value description updated
- DT_BRACKETS: Competition /Bracket /BracketItems /BracketItem /CompetitorPlace /PreviousUnit Value updated
- DT_POOL_STANDINGS: Structure table corrected
- DT_PRESENTER: Editorial updates to match current implementation
- DT_ENTRIES: Competition /Entry Attributes: Code, Type, Organisation and SortOrder updated to optional to support the individual generic events.
- DT_LOCAL_ON: Element Competition: Gen, Sport and Codes attributes added.

### `2026-4.4` (APP)
- DT_VEN_COND: Trigger and Frequency updated.
- Venue /DateTime /Condititons /Precipitation and Venue /DateTime/Conditions/Pressure added back (CHG0034175)
- DT_WEATHER: Value format for Temperature and Precipitation adjusted (CHG0034175)
- DT_BRACKETS: Message Sort: Clarification added for consistency
- Competition /Bracket /BracketItems /BracketItem /CompetitorPlace /PreviousUnit unit value update to support phases
- DT_SCHEDULE: Competition /Unit /StartList /Start /PreviousUnit unit value update to support phases; Competition /Unit description for the non-competition units unit pattern to be used updated.
- DT_ACHIEVEMENT: Samples correction.
- DT_FLAGBEARERS: Structure updated to include Cluster information and overall reviewed. Sample updated
- DT_NEWS/ DT_NEWS_IMP: Category attribute description updated to provide clarity.
- DT_BCK: SortOrder attribute value updated to S(8)
- DT_BIO_TEA/DT_BIO_TEA_IMP: Elements Club_Name, Coach and Debut added
- DT_IMAGE: Header Values and Description updated to extend the use of the message for the distribution of Flag images.
- DT_ENTRIES: Competition /Entry Attributes: Code, Type, Organisation and SortOrder updated to Mandatory. Change applied in the previous version reverted.

### `2026-4.5` (APP)
- DT_RESULT: Competition /Result /Competitor /Coaches /Coach /Description attribute IFId added for consistency.
- DT_RESULT_ANALYSIS: Competition /Result /Competitor /Coaches /Coach /Description attribute IFId added for consistency.
- DT_PLAY_BY_PLAY: Competition /Actions /Action /Competitor /Coaches /Coach /Description attribute IFId added for consistency.
- DT_IMAGE:
- Competition /Image /Result attributes RankEqual and Diff added for consistency
- Competition /Image /Result /Competitor attributes Code and Type updated as Mandatory. Attribute Bib added for consistency.
- Competition /Image /Result /Competitor /Composition /Athlete attribute code updated as Mandatory for consistency.
- Competition /Image /Result /Competitor /Description attribute IFId added for consistency.
- Competition /Image /Result /Competitor /Composition /Athlete /Description attributes Gender, Organisation, Birthday, IFId added for consistency.
- ImageType svg added and descriptions updated (CHG0035534)
- DT_STATS:
- Competition /Officials /Official /Description attribute IFId added for consistency
- Competition /Stats /Competitor /Coaches /Coach /Description attributes Nationality and IFId added for consistency.
- DT_FED_RANKING: Correction in the structure and the attribute's value reference and descriptions.
- DT_WEATHER: Editorial update.
- DT_PRESENTER:
- Header Values: Correction in the Competition code to match implementation.
- Competition /Presentation attributes Event and EventName correction in Value and description adjustments to match implementation.
- Competition /Presentation /Presenter attribute MedalRank value corrected.
- DT_MEDALS:
- Description updated for clarity, Sample updated.
- Competition /MedalStandings /MedalsTable /MedalLine /Description attribute OrganisationName value updated to match implementation.
- DT_BIO_PAR: Competition /ParticipantBiography /Language /GInterest /ExtendedBios /ExtendedBio attributed ContentType and - deleted (DDM-119), editorial updates.
- DT_BIO_PAR_IMP: Competition/ ParticipantBiography /Language /GInterest /ExtendedBios /ExtendedBio attributes ContentType and - deleted (DDM-119), editorial updates.
- DT_BIO_NOC, DT_BIO_NOC_IMP Element: Competition/ Organisation /Language /GInterest (0,1) /Highlights Value length increased, editorial updates.
- DT_BIO_NOC_IMP Element: Competition /Organisation attribute Name deleted, attribute Code added.
- DT_ACHIEVEMENT: Competition /Competitor /Achievement attribute Highlight Value updated to S(60)
- DT_FLAGBEARERS: Ceremony /Flagbearer attribute Code set as Optional in the case that a Volunteer is used.
- DT_ALERT: Header Values: Language added to match implementation.
- DT_TV_TRACKING: Header Values: Correction of typo.
- DT_NEWS and DT_NEWS_IMP :Document /Related /Coach /Description attribute IFId added for consistency
- DT_ESL:
- Competition /Result /Competitor /Coaches /Coach /Description attribute IFId added for consistency.
- Elements cardinality updated for consistency with DT_RESULT.
- DT_AUDIO: Competition /Audio /Related /Coach /Description attribute IFId added for consistency, structure correction.
- DT_SCHED_RES_NOC:
- Competition /Unit attribute HideUnitNum added for consistency with DT_SCHEDULE.
- Competition /Unit /Result /Competitor attribute type description updated.
- Editorial updates.
- DT_CODES:
- Attributes: Note, Description, ShortDescription and LongDescription values updated to S(255) as the maximum length to be used. Definition of each length in each codeset is defined in the Common Codes Definition requirements.
- Attribute Medal value updated to S(1).

### `2026-4.6` (APP)
- DT_ACHIEVEMENTS: Competition /Competitor @Name Value updated to S(73) to accommodate Team Name.
- DT_IMAGE: Header Values: DocumentSubcode updated to ORG to accommodate the IF flags distribution.
- DT_POOL_STANDING: Trigger and Frequency: editorial update.
- Competition /Result: @Won @Lost @Tied @Played @For @Against values updated to refer to Sport Specific definition
- DT_RANKING: Trigger and Frequency: editorial update.
- DT_RECORD: Competition /Record /Description Value updated to refer to the Common Codes description
- DT_SCHEDULE: Message sample correction.
- DT_CUMULATIVE_RESULT: Header Values: DocumentCode updated.
- DT_PIC: Competition /ExtendedInfos /ExtendedInfo added.
- DT_AUDIO: Competition /ExtendedInfos /ExtendedInfo added
- DT_BIO_PAR, DT_BIO_TEA, DT_BIO_NOC value text references updated from 20,000 to 40,000 characters
- DT_CODES: Competition /Codeset @VenueCode value updated to accept S(20).

## Modeling Notes

- The 2026 general interface is not just editorial relative to Paris 2024. Versions `2026-4.0` through `2026-4.6` introduce structural changes across message headers, participant messages, entries, schedule, result, image, code-set and biography messages.
- Several `DT_SCHEDULE` changes directly affect Sportivo modeling: `PreviousValue` in start-list rows (`2026-4.0`), non-competition unit handling and `MediaAccess` (`2026-4.3`/`2026-4.4`), `DocumentSubtype=PRE` (`2026-4.3`), `PreviousUnit` support for phases (`2026-4.4`), and sample correction (`2026-4.6`).
- XSD mismatches should be expected when local schemas were generated from a different publication point than `OWG2026-GEN-4.6`. Use the PDF Document Control version as the source of truth when validating whether a mismatch is drift, typo, or an implementation gap.
