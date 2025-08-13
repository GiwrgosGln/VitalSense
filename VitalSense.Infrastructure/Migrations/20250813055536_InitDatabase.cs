using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitalSense.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "appointments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    start = table.Column<DateTime>(type: "datetime2", nullable: false),
                    end = table.Column<DateTime>(type: "datetime2", nullable: false),
                    all_day = table.Column<bool>(type: "bit", nullable: false),
                    dietician_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    client_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appointments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "clients",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    first_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    last_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    date_of_birth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    gender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    has_card = table.Column<bool>(type: "bit", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    dietician_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clients", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "meal_plans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    start_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    end_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    dietician_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    client_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meal_plans", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tasks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_completed = table.Column<bool>(type: "bit", nullable: false),
                    due_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    dietician_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    client_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tasks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    password_salt = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    last_login = table.Column<DateTime>(type: "datetime2", nullable: true),
                    refresh_token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    refresh_token_expiry = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "meal_days",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    meal_plan_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meal_days", x => x.id);
                    table.ForeignKey(
                        name: "FK_meal_days_meal_plans_meal_plan_id",
                        column: x => x.meal_plan_id,
                        principalTable: "meal_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "meals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    time = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    protein = table.Column<float>(type: "real", nullable: false),
                    carbs = table.Column<float>(type: "real", nullable: false),
                    fats = table.Column<float>(type: "real", nullable: false),
                    calories = table.Column<float>(type: "real", nullable: false),
                    meal_day_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meals", x => x.id);
                    table.ForeignKey(
                        name: "FK_meals_meal_days_meal_day_id",
                        column: x => x.meal_day_id,
                        principalTable: "meal_days",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_meal_days_meal_plan_id",
                table: "meal_days",
                column: "meal_plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_meals_meal_day_id",
                table: "meals",
                column: "meal_day_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "appointments");

            migrationBuilder.DropTable(
                name: "clients");

            migrationBuilder.DropTable(
                name: "meals");

            migrationBuilder.DropTable(
                name: "tasks");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "meal_days");

            migrationBuilder.DropTable(
                name: "meal_plans");
        }
    }
}
