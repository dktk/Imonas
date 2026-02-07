using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserIds2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {   
            migrationBuilder.AlterColumn<string>(
                name: "user_id",
                schema: "silver",
                table: "internal_payments",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "ix_internal_payments_user_id",
                schema: "silver",
                table: "internal_payments",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_internal_payments_user_id",
                schema: "silver",
                table: "internal_payments");

            migrationBuilder.AlterColumn<int>(
                name: "user_id",
                schema: "silver",
                table: "internal_payments",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
