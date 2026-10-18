using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafetyCopilot.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tblUser",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OpenAiApiKeyEncrypted = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblUser", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tblProject",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblProject", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblProject_tblUser_UserId",
                        column: x => x.UserId,
                        principalTable: "tblUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblRequirementDocument",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    PdfContent = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    ExtractedText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblRequirementDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblRequirementDocument_tblProject_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tblProject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblRequirement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequirementNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RequirementText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblRequirement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblRequirement_tblRequirementDocument_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "tblRequirementDocument",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblAiClassification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequirementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsHazard = table.Column<bool>(type: "bit", nullable: false),
                    Confidence = table.Column<double>(type: "float", nullable: true),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModelName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ClassifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblAiClassification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblAiClassification_tblRequirement_RequirementId",
                        column: x => x.RequirementId,
                        principalTable: "tblRequirement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblHumanClassification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequirementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsHazard = table.Column<bool>(type: "bit", nullable: false),
                    ClassifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblHumanClassification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblHumanClassification_tblRequirement_RequirementId",
                        column: x => x.RequirementId,
                        principalTable: "tblRequirement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tblHumanClassification_tblUser_UserId",
                        column: x => x.UserId,
                        principalTable: "tblUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tblAiClassification_RequirementId",
                table: "tblAiClassification",
                column: "RequirementId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblHumanClassification_RequirementId",
                table: "tblHumanClassification",
                column: "RequirementId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblHumanClassification_UserId",
                table: "tblHumanClassification",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tblProject_UserId",
                table: "tblProject",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tblRequirement_DocumentId_SequenceNumber",
                table: "tblRequirement",
                columns: new[] { "DocumentId", "SequenceNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_tblRequirementDocument_ProjectId",
                table: "tblRequirementDocument",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_tblUser_Email",
                table: "tblUser",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tblAiClassification");

            migrationBuilder.DropTable(
                name: "tblHumanClassification");

            migrationBuilder.DropTable(
                name: "tblRequirement");

            migrationBuilder.DropTable(
                name: "tblRequirementDocument");

            migrationBuilder.DropTable(
                name: "tblProject");

            migrationBuilder.DropTable(
                name: "tblUser");
        }
    }
}
