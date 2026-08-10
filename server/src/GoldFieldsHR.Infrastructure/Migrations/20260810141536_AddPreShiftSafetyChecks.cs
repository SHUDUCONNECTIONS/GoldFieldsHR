using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldFieldsHR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPreShiftSafetyChecks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PreShiftSafetyChecks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckDate = table.Column<DateOnly>(type: "date", nullable: false),
                    HazardsIdentified = table.Column<bool>(type: "boolean", nullable: false),
                    HazardNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                name: "IX_PreShiftSafetyChecks_EmployeeId_CheckDate",
                table: "PreShiftSafetyChecks",
                columns: new[] { "EmployeeId", "CheckDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PreShiftSafetyChecks");
        }
    }
}
