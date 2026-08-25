using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SessionTrackerAPi.Migrations
{
    /// <inheritdoc />
    public partial class AddPricingAndDuration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DurationInHours",
                table: "Sessions",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<decimal>(
                name: "HourlyRate",
                table: "Sessions",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationInHours",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "HourlyRate",
                table: "Sessions");
        }
    }
}
