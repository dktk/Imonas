using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddArchiveToReconciliationRuns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                table: "reconciliation_runs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "archived_at",
                table: "reconciliation_runs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "archived_by",
                table: "reconciliation_runs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "archive_comment",
                table: "reconciliation_runs",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_archived",
                table: "reconciliation_runs");

            migrationBuilder.DropColumn(
                name: "archived_at",
                table: "reconciliation_runs");

            migrationBuilder.DropColumn(
                name: "archived_by",
                table: "reconciliation_runs");

            migrationBuilder.DropColumn(
                name: "archive_comment",
                table: "reconciliation_runs");
        }
    }
}
