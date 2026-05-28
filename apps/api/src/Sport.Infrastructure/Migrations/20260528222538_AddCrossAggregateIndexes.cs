using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sport.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCrossAggregateIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_official_assignments_person_id",
                table: "official_assignments",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "ix_entries_organisation_id",
                table: "entries",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_entries_team_id",
                table: "entries",
                column: "team_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_official_assignments_person_id",
                table: "official_assignments");

            migrationBuilder.DropIndex(
                name: "ix_entries_organisation_id",
                table: "entries");

            migrationBuilder.DropIndex(
                name: "ix_entries_team_id",
                table: "entries");
        }
    }
}
