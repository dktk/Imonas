using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AttachmentsInDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "file_path",
                table: "case_attachments");

            migrationBuilder.AddColumn<byte[]>(
                name: "file_content",
                table: "case_attachments",
                type: "bytea",
                nullable: false,
                defaultValue: Array.Empty<byte>());
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "file_content",
                table: "case_attachments");

            migrationBuilder.AddColumn<string>(
                name: "file_path",
                table: "case_attachments",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
