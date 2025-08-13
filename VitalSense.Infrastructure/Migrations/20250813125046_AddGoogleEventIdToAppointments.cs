using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitalSense.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleEventIdToAppointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "google_event_id",
                table: "appointments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "google_event_id",
                table: "appointments");
        }
    }
}
