using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitalSense.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionnaireTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "questionnaire_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    dietician_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questionnaire_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "questionnaire_questions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    template_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    question_text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    order = table.Column<int>(type: "int", nullable: false),
                    is_required = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questionnaire_questions", x => x.id);
                    table.ForeignKey(
                        name: "FK_questionnaire_questions_questionnaire_templates_template_id",
                        column: x => x.template_id,
                        principalTable: "questionnaire_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_questionnaire_questions_template_id",
                table: "questionnaire_questions",
                column: "template_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "questionnaire_questions");

            migrationBuilder.DropTable(
                name: "questionnaire_templates");
        }
    }
}
