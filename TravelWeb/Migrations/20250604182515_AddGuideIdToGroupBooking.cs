using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddGuideIdToGroupBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GuideId",
                table: "GroupBookings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupBookings_GuideId",
                table: "GroupBookings",
                column: "GuideId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupBookings_Guides_GuideId",
                table: "GroupBookings",
                column: "GuideId",
                principalTable: "Guides",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupBookings_Guides_GuideId",
                table: "GroupBookings");

            migrationBuilder.DropIndex(
                name: "IX_GroupBookings_GuideId",
                table: "GroupBookings");

            migrationBuilder.DropColumn(
                name: "GuideId",
                table: "GroupBookings");
        }
    }
}
