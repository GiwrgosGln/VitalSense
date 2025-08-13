using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitalSense.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleCalendarIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "google_access_token",
                table: "users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "google_refresh_token",
                table: "users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "google_token_expiry",
                table: "users",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "google_access_token",
                table: "users");

            migrationBuilder.DropColumn(
                name: "google_refresh_token",
                table: "users");

            migrationBuilder.DropColumn(
                name: "google_token_expiry",
                table: "users");
        }
    }
}
