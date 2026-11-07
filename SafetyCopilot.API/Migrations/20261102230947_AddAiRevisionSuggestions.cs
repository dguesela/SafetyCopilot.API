using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafetyCopilot.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAiRevisionSuggestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RevisionNeeded",
                table: "tblAiClassification",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RevisionReason",
                table: "tblAiClassification",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuggestedRevision",
                table: "tblAiClassification",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RevisionNeeded",
                table: "tblAiClassification");

            migrationBuilder.DropColumn(
                name: "RevisionReason",
                table: "tblAiClassification");

            migrationBuilder.DropColumn(
                name: "SuggestedRevision",
                table: "tblAiClassification");
        }
    }
}
