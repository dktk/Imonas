using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Configs1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_psp_configurations_psp_id",
                table: "psp_configurations");

            migrationBuilder.AlterColumn<string>(
                name: "config_json",
                table: "psp_configurations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "{}");

            migrationBuilder.CreateIndex(
                name: "ix_psp_configurations_psp_id",
                table: "psp_configurations",
                column: "psp_id");

            migrationBuilder.CreateIndex(
                name: "ix_psp_configurations_user_id",
                table: "psp_configurations",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_psp_configurations_psp_id",
                table: "psp_configurations");

            migrationBuilder.DropIndex(
                name: "ix_psp_configurations_user_id",
                table: "psp_configurations");

            migrationBuilder.AlterColumn<string>(
                name: "config_json",
                table: "psp_configurations",
                type: "text",
                nullable: false,
                defaultValue: "{}",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_psp_configurations_psp_id",
                table: "psp_configurations",
                column: "psp_id",
                unique: true);
        }
    }
}
