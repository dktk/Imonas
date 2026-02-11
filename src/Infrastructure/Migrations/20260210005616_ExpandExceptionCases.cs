using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExpandExceptionCases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "linked_transaction_id",
                table: "exception_cases",
                newName: "internal_transaction_id");

            migrationBuilder.AddColumn<int>(
                name: "external_transaction_id",
                table: "exception_cases",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "external_transaction_id",
                table: "exception_cases");

            migrationBuilder.RenameColumn(
                name: "internal_transaction_id",
                table: "exception_cases",
                newName: "linked_transaction_id");
        }
    }
}
