using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldFieldsHR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSafetyAndEmergencyTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmergencyAlerts");

            migrationBuilder.DropTable(
                name: "PreShiftSafetyChecks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmergencyAlerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ResolutionNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ResolvedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedByEmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TriggeredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmergencyAlerts_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PreShiftSafetyChecks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckDate = table.Column<DateOnly>(type: "date", nullable: false),
                    HazardNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    HazardsIdentified = table.Column<bool>(type: "boolean", nullable: false),
                    SubmittedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreShiftSafetyChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreShiftSafetyChecks_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyAlerts_EmployeeId",
                table: "EmergencyAlerts",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyAlerts_Status",
                table: "EmergencyAlerts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PreShiftSafetyChecks_EmployeeId_CheckDate",
                table: "PreShiftSafetyChecks",
                columns: new[] { "EmployeeId", "CheckDate" },
                unique: true);
        }
    }
}
