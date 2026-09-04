using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SessionTrackerAPi.Migrations
{
    /// <inheritdoc />
    public partial class AddPaidNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaidNote",
                table: "Sessions",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaidNote",
                table: "Sessions");
        }
    }
}
