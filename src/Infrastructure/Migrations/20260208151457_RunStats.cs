using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RunStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "unmatched_records",
                table: "reconciliation_runs",
                newName: "partial_match_records_count");

            migrationBuilder.RenameColumn(
                name: "total_records",
                table: "reconciliation_runs",
                newName: "internal_unmatched_records_count");

            migrationBuilder.RenameColumn(
                name: "partial_match_records",
                table: "reconciliation_runs",
                newName: "internal_matched_records_count");

            migrationBuilder.RenameColumn(
                name: "matched_records",
                table: "reconciliation_runs",
                newName: "external_unmatched_records_count");

            migrationBuilder.AddColumn<int>(
                name: "external_matched_records_count",
                table: "reconciliation_runs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "external_matched_records_count",
                table: "reconciliation_runs");

            migrationBuilder.RenameColumn(
                name: "partial_match_records_count",
                table: "reconciliation_runs",
                newName: "unmatched_records");

            migrationBuilder.RenameColumn(
                name: "internal_unmatched_records_count",
                table: "reconciliation_runs",
                newName: "total_records");

            migrationBuilder.RenameColumn(
                name: "internal_matched_records_count",
                table: "reconciliation_runs",
                newName: "partial_match_records");

            migrationBuilder.RenameColumn(
                name: "external_unmatched_records_count",
                table: "reconciliation_runs",
                newName: "matched_records");
        }
    }
}
