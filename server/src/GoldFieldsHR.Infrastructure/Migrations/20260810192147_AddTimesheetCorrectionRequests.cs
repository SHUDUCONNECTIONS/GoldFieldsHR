using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldFieldsHR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTimesheetCorrectionRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TimesheetCorrectionRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TimesheetEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedClockInUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RequestedClockOutUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewerId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimesheetCorrectionRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimesheetCorrectionRequests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimesheetCorrectionRequests_TimesheetEntries_TimesheetEntry~",
                        column: x => x.TimesheetEntryId,
                        principalTable: "TimesheetEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TimesheetCorrectionRequests_EmployeeId",
                table: "TimesheetCorrectionRequests",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_TimesheetCorrectionRequests_Status",
                table: "TimesheetCorrectionRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TimesheetCorrectionRequests_TimesheetEntryId",
                table: "TimesheetCorrectionRequests",
                column: "TimesheetEntryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimesheetCorrectionRequests");
        }
    }
}
