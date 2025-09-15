using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitalSense.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionnaireSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "questionnaire_submissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    template_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    dietician_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    client_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    submitted_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questionnaire_submissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_questionnaire_submissions_questionnaire_templates_template_id",
                        column: x => x.template_id,
                        principalTable: "questionnaire_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "questionnaire_answers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    submission_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    question_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    answer_text = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questionnaire_answers", x => x.id);
                    table.ForeignKey(
                        name: "FK_questionnaire_answers_questionnaire_submissions_submission_id",
                        column: x => x.submission_id,
                        principalTable: "questionnaire_submissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_questionnaire_answers_submission_id",
                table: "questionnaire_answers",
                column: "submission_id");

            migrationBuilder.CreateIndex(
                name: "IX_questionnaire_submissions_template_id",
                table: "questionnaire_submissions",
                column: "template_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "questionnaire_answers");

            migrationBuilder.DropTable(
                name: "questionnaire_submissions");
        }
    }
}
