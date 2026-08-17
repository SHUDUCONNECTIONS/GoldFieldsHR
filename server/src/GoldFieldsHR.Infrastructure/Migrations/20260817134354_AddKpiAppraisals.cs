using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldFieldsHR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKpiAppraisals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KpiTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Designation = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KpiTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KpiAppraisals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    KpiTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodLabel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InductionNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedByEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Checkpoint1Date = table.Column<DateOnly>(type: "date", nullable: true),
                    Checkpoint2Date = table.Column<DateOnly>(type: "date", nullable: true),
                    Checkpoint3Date = table.Column<DateOnly>(type: "date", nullable: true),
                    Checkpoint4Date = table.Column<DateOnly>(type: "date", nullable: true),
                    BlastingOfficerEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    BlastingOfficerSignedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BlastingOfficerSignatureImageData = table.Column<byte[]>(type: "bytea", nullable: true),
                    BlastingEngineerEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    BlastingEngineerSignedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BlastingEngineerSignatureImageData = table.Column<byte[]>(type: "bytea", nullable: true),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastScoredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FinalizedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KpiAppraisals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KpiAppraisals_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KpiAppraisals_KpiTemplates_KpiTemplateId",
                        column: x => x.KpiTemplateId,
                        principalTable: "KpiTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KpiTemplateCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KpiTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KpiTemplateCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KpiTemplateCategories_KpiTemplates_KpiTemplateId",
                        column: x => x.KpiTemplateId,
                        principalTable: "KpiTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KpiTemplateItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KpiTemplateCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SubGroupLabel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KpiTemplateItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KpiTemplateItems_KpiTemplateCategories_KpiTemplateCategoryId",
                        column: x => x.KpiTemplateCategoryId,
                        principalTable: "KpiTemplateCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KpiAppraisalItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KpiAppraisalId = table.Column<Guid>(type: "uuid", nullable: false),
                    KpiTemplateItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    DescriptionSnapshot = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CategoryNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SubGroupLabelSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    InPlace = table.Column<bool>(type: "boolean", nullable: true),
                    Ability = table.Column<bool>(type: "boolean", nullable: true),
                    Checkpoint1Score = table.Column<int>(type: "integer", nullable: true),
                    Checkpoint1Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Checkpoint2Score = table.Column<int>(type: "integer", nullable: true),
                    Checkpoint2Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Checkpoint3Score = table.Column<int>(type: "integer", nullable: true),
                    Checkpoint3Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Checkpoint4Score = table.Column<int>(type: "integer", nullable: true),
                    Checkpoint4Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Evaluation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KpiAppraisalItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KpiAppraisalItems_KpiAppraisals_KpiAppraisalId",
                        column: x => x.KpiAppraisalId,
                        principalTable: "KpiAppraisals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KpiAppraisalItems_KpiTemplateItems_KpiTemplateItemId",
                        column: x => x.KpiTemplateItemId,
                        principalTable: "KpiTemplateItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KpiAppraisalItems_KpiAppraisalId",
                table: "KpiAppraisalItems",
                column: "KpiAppraisalId");

            migrationBuilder.CreateIndex(
                name: "IX_KpiAppraisalItems_KpiTemplateItemId",
                table: "KpiAppraisalItems",
                column: "KpiTemplateItemId");

            migrationBuilder.CreateIndex(
                name: "IX_KpiAppraisals_EmployeeId",
                table: "KpiAppraisals",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_KpiAppraisals_KpiTemplateId",
                table: "KpiAppraisals",
                column: "KpiTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_KpiAppraisals_Status",
                table: "KpiAppraisals",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_KpiTemplateCategories_KpiTemplateId",
                table: "KpiTemplateCategories",
                column: "KpiTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_KpiTemplateItems_KpiTemplateCategoryId",
                table: "KpiTemplateItems",
                column: "KpiTemplateCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KpiAppraisalItems");

            migrationBuilder.DropTable(
                name: "KpiAppraisals");

            migrationBuilder.DropTable(
                name: "KpiTemplateItems");

            migrationBuilder.DropTable(
                name: "KpiTemplateCategories");

            migrationBuilder.DropTable(
                name: "KpiTemplates");
        }
    }
}
