using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldFieldsHR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSignaturesAndLeaveHrApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReviewerId",
                table: "LeaveRequests",
                newName: "LineManagerReviewerId");

            migrationBuilder.RenameColumn(
                name: "ReviewedAtUtc",
                table: "LeaveRequests",
                newName: "LineManagerReviewedAtUtc");

            migrationBuilder.AddColumn<byte[]>(
                name: "SignatureImageData",
                table: "PolicyAcknowledgments",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "LeaveRequests",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            // LeaveRequestStatus gained an HR-approval stage: the old single-stage "Pending"
            // becomes "PendingLineManagerApproval" so existing pending requests still route
            // through line manager review first, exactly as before.
            migrationBuilder.Sql(
                """UPDATE "LeaveRequests" SET "Status" = 'PendingLineManagerApproval' WHERE "Status" = 'Pending'""");

            migrationBuilder.AddColumn<DateTime>(
                name: "HRReviewedAtUtc",
                table: "LeaveRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HRReviewerId",
                table: "LeaveRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "HRSignatureImageData",
                table: "LeaveRequests",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "LineManagerSignatureImageData",
                table: "LeaveRequests",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "SignatureImageData",
                table: "Employees",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SignatureUpdatedAtUtc",
                table: "Employees",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SignatureImageData",
                table: "PolicyAcknowledgments");

            migrationBuilder.DropColumn(
                name: "HRReviewedAtUtc",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "HRReviewerId",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "HRSignatureImageData",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "LineManagerSignatureImageData",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "SignatureImageData",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "SignatureUpdatedAtUtc",
                table: "Employees");

            migrationBuilder.Sql(
                """UPDATE "LeaveRequests" SET "Status" = 'Pending' WHERE "Status" IN ('PendingLineManagerApproval', 'PendingHRApproval')""");

            migrationBuilder.RenameColumn(
                name: "LineManagerReviewerId",
                table: "LeaveRequests",
                newName: "ReviewerId");

            migrationBuilder.RenameColumn(
                name: "LineManagerReviewedAtUtc",
                table: "LeaveRequests",
                newName: "ReviewedAtUtc");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "LeaveRequests",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);
        }
    }
}
