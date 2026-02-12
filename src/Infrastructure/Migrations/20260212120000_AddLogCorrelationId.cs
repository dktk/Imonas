using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLogCorrelationId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "correlation_id",
                table: "serilogs",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_serilogs_correlation_id",
                table: "serilogs",
                column: "correlation_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_serilogs_correlation_id",
                table: "serilogs");

            migrationBuilder.DropColumn(
                name: "correlation_id",
                table: "serilogs");
        }
    }
}
