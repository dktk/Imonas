using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExceptionCasesChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "assigned_to",
                table: "exception_cases",
                newName: "assigned_to_id");

            migrationBuilder.CreateIndex(
                name: "ix_exception_cases_assigned_to_id",
                table: "exception_cases",
                column: "assigned_to_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_exception_cases_assigned_to_id",
                table: "exception_cases");

            migrationBuilder.RenameColumn(
                name: "assigned_to_id",
                table: "exception_cases",
                newName: "assigned_to");
        }
    }
}
