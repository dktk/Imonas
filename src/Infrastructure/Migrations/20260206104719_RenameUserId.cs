using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_by",
                schema: "silver",
                table: "internal_payments");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "status_mappings",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "run_metrics",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "reconciliation_schedules",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "reconciliation_runs",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "created_by",
                schema: "gold",
                table: "reconciliation_comments",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "created_by",
                schema: "bronze",
                table: "raw_payments",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "psps",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "created_by",
                schema: "gold",
                table: "psp_settlements",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "notifications",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "matching_rules",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "key_values",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "invoices",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "invoice_raw_datas",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "internal_systems",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "field_mappings",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "fee_contracts",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "created_by",
                schema: "silver",
                table: "external_payments",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "exception_cases",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "documents",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "document_types",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "customers",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "case_labels",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "case_comments",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "case_attachments",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "approval_datas",
                newName: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_raw_payments_user_id",
                schema: "bronze",
                table: "raw_payments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_psps_user_id",
                table: "psps",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_raw_payments_user_id",
                schema: "bronze",
                table: "raw_payments");

            migrationBuilder.DropIndex(
                name: "ix_psps_user_id",
                table: "psps");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "status_mappings",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "run_metrics",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "reconciliation_schedules",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "reconciliation_runs",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "gold",
                table: "reconciliation_comments",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "bronze",
                table: "raw_payments",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "psps",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "gold",
                table: "psp_settlements",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "notifications",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "matching_rules",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "key_values",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "invoices",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "invoice_raw_datas",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "internal_systems",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "field_mappings",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "fee_contracts",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "silver",
                table: "external_payments",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "exception_cases",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "documents",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "document_types",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "customers",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "case_labels",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "case_comments",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "case_attachments",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "approval_datas",
                newName: "created_by");

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                schema: "silver",
                table: "internal_payments",
                type: "text",
                nullable: true);
        }
    }
}
