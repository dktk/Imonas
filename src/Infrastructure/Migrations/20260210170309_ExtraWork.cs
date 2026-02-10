using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExtraWork : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "file_content",
                table: "case_attachments",
                type: "bytea",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "bytea");

            migrationBuilder.CreateIndex(
                name: "ix_exception_cases_reconciliation_run_id",
                table: "exception_cases",
                column: "reconciliation_run_id");

            migrationBuilder.AddForeignKey(
                name: "fk_exception_cases_reconciliation_runs_reconciliation_run_id",
                table: "exception_cases",
                column: "reconciliation_run_id",
                principalTable: "reconciliation_runs",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_exception_cases_reconciliation_runs_reconciliation_run_id",
                table: "exception_cases");

            migrationBuilder.DropIndex(
                name: "ix_exception_cases_reconciliation_run_id",
                table: "exception_cases");

            migrationBuilder.AlterColumn<byte[]>(
                name: "file_content",
                table: "case_attachments",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldNullable: true);
        }
    }
}
