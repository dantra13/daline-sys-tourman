using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sport.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "competitions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    dates_end = table.Column<DateOnly>(type: "date", nullable: false),
                    dates_start = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_competitions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    team_id = table.Column<Guid>(type: "uuid", nullable: true),
                    bib = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    seed = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    competition_discipline_id = table.Column<Guid>(type: "uuid", nullable: false),
                    discipline_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    gender = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    event_type = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    event_modifier = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    rsc = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "official_assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    function_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    scope_level = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    scope_target_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_official_assignments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "organisations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organisations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "persons",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    given_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    gender = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: true),
                    if_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_persons", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "teams",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    discipline_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_teams", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "competition_disciplines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    competition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    enabled_genders = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_competition_disciplines", x => x.id);
                    table.ForeignKey(
                        name: "fk_competition_disciplines_competitions_competition_id",
                        column: x => x.competition_id,
                        principalTable: "competitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "composition_members",
                columns: table => new
                {
                    entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    bib = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_composition_members", x => new { x.entry_id, x.person_id });
                    table.ForeignKey(
                        name: "fk_composition_members_entries_entry_id",
                        column: x => x.entry_id,
                        principalTable: "entries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "phases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    rsc = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_phases", x => x.id);
                    table.ForeignKey(
                        name: "fk_phases_events_event_id",
                        column: x => x.event_id,
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "units",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    phase_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    scheduled_start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    rsc = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: false),
                    discipline_unit_ref = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_units", x => x.id);
                    table.ForeignKey(
                        name: "fk_units_phases_phase_id",
                        column: x => x.phase_id,
                        principalTable: "phases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subunits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    unit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    rsc = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subunits", x => x.id);
                    table.ForeignKey(
                        name: "fk_subunits_units_unit_id",
                        column: x => x.unit_id,
                        principalTable: "units",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_competition_disciplines_competition_id_code",
                table: "competition_disciplines",
                columns: new[] { "competition_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_competitions_code",
                table: "competitions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_composition_members_person_id_entry_id",
                table: "composition_members",
                columns: new[] { "person_id", "entry_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_entries_event_id_status",
                table: "entries",
                columns: new[] { "event_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_events_competition_discipline_id_rsc",
                table: "events",
                columns: new[] { "competition_discipline_id", "rsc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_organisations_code",
                table: "organisations",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_persons_family_name_given_name",
                table: "persons",
                columns: new[] { "family_name", "given_name" });

            migrationBuilder.CreateIndex(
                name: "ix_phases_event_id_code",
                table: "phases",
                columns: new[] { "event_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_phases_event_id_order",
                table: "phases",
                columns: new[] { "event_id", "order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_subunits_unit_id_code",
                table: "subunits",
                columns: new[] { "unit_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_teams_code",
                table: "teams",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_units_phase_id_code",
                table: "units",
                columns: new[] { "phase_id", "code" },
                unique: true);

            // Composite index over ComplexProperty columns — cannot be expressed via HasIndex fluent API.
            migrationBuilder.Sql(
                "CREATE INDEX ix_official_assignments_scope_function ON official_assignments (scope_level, scope_target_id, function_code);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "competition_disciplines");

            migrationBuilder.DropTable(
                name: "composition_members");

            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS ix_official_assignments_scope_function;");

            migrationBuilder.DropTable(
                name: "official_assignments");

            migrationBuilder.DropTable(
                name: "organisations");

            migrationBuilder.DropTable(
                name: "persons");

            migrationBuilder.DropTable(
                name: "subunits");

            migrationBuilder.DropTable(
                name: "teams");

            migrationBuilder.DropTable(
                name: "competitions");

            migrationBuilder.DropTable(
                name: "entries");

            migrationBuilder.DropTable(
                name: "units");

            migrationBuilder.DropTable(
                name: "phases");

            migrationBuilder.DropTable(
                name: "events");
        }
    }
}
