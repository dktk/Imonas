using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ClientIdRefactoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ref_number",
                schema: "silver",
                table: "internal_payments",
                newName: "reference_code");

            migrationBuilder.AddColumn<int>(
                name: "client_id",
                schema: "silver",
                table: "internal_payments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                "ALTER TABLE silver.external_payments ALTER COLUMN player_id TYPE integer USING player_id::integer;");

            migrationBuilder.AddColumn<string>(
                name: "description",
                schema: "silver",
                table: "external_payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email",
                schema: "silver",
                table: "external_payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "reference_code",
                schema: "silver",
                table: "external_payments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "client_id",
                schema: "silver",
                table: "internal_payments");

            migrationBuilder.DropColumn(
                name: "description",
                schema: "silver",
                table: "external_payments");

            migrationBuilder.DropColumn(
                name: "email",
                schema: "silver",
                table: "external_payments");

            migrationBuilder.DropColumn(
                name: "reference_code",
                schema: "silver",
                table: "external_payments");

            migrationBuilder.RenameColumn(
                name: "reference_code",
                schema: "silver",
                table: "internal_payments",
                newName: "ref_number");

            migrationBuilder.Sql(
                "ALTER TABLE silver.external_payments ALTER COLUMN player_id TYPE text USING player_id::text;");
        }
    }
}
