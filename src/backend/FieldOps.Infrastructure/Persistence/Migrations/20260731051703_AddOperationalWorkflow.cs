using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FieldOps.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationalWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AssignedAt",
                table: "work_orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedTechnicianId",
                table: "work_orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientReopenReason",
                table: "work_orders",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CompletedAt",
                table: "work_orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompletionSummary",
                table: "work_orders",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StartedAt",
                table: "work_orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SubmittedForApprovalAt",
                table: "work_orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClientUserId",
                table: "customers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_user_accounts_TenantId_Id",
                table: "user_accounts",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_TenantId_AssignedTechnicianId_Status",
                table: "work_orders",
                columns: new[] { "TenantId", "AssignedTechnicianId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_customers_TenantId_ClientUserId",
                table: "customers",
                columns: new[] { "TenantId", "ClientUserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_customers_user_accounts_TenantId_ClientUserId",
                table: "customers",
                columns: new[] { "TenantId", "ClientUserId" },
                principalTable: "user_accounts",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_work_orders_user_accounts_TenantId_AssignedTechnicianId",
                table: "work_orders",
                columns: new[] { "TenantId", "AssignedTechnicianId" },
                principalTable: "user_accounts",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_customers_user_accounts_TenantId_ClientUserId",
                table: "customers");

            migrationBuilder.DropForeignKey(
                name: "FK_work_orders_user_accounts_TenantId_AssignedTechnicianId",
                table: "work_orders");

            migrationBuilder.DropIndex(
                name: "IX_work_orders_TenantId_AssignedTechnicianId_Status",
                table: "work_orders");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_user_accounts_TenantId_Id",
                table: "user_accounts");

            migrationBuilder.DropIndex(
                name: "IX_customers_TenantId_ClientUserId",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "AssignedAt",
                table: "work_orders");

            migrationBuilder.DropColumn(
                name: "AssignedTechnicianId",
                table: "work_orders");

            migrationBuilder.DropColumn(
                name: "ClientReopenReason",
                table: "work_orders");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "work_orders");

            migrationBuilder.DropColumn(
                name: "CompletionSummary",
                table: "work_orders");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "work_orders");

            migrationBuilder.DropColumn(
                name: "SubmittedForApprovalAt",
                table: "work_orders");

            migrationBuilder.DropColumn(
                name: "ClientUserId",
                table: "customers");
        }
    }
}
