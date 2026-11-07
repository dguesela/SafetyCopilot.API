using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafetyCopilot.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRevisionDecisions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RevisionDecision",
                table: "tblAiClassification",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RevisionReviewedAtUtc",
                table: "tblAiClassification",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RevisionDecision",
                table: "tblAiClassification");

            migrationBuilder.DropColumn(
                name: "RevisionReviewedAtUtc",
                table: "tblAiClassification");
        }
    }
}
