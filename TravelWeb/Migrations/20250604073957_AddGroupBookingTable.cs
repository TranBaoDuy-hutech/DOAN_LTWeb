using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupBookingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_GroupBooking_GroupBookingId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupBooking_Tours_TourId",
                table: "GroupBooking");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupBooking",
                table: "GroupBooking");

            migrationBuilder.RenameTable(
                name: "GroupBooking",
                newName: "GroupBookings");

            migrationBuilder.RenameIndex(
                name: "IX_GroupBooking_TourId",
                table: "GroupBookings",
                newName: "IX_GroupBookings_TourId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupBookings",
                table: "GroupBookings",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_GroupBookings_GroupBookingId",
                table: "Bookings",
                column: "GroupBookingId",
                principalTable: "GroupBookings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupBookings_Tours_TourId",
                table: "GroupBookings",
                column: "TourId",
                principalTable: "Tours",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_GroupBookings_GroupBookingId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupBookings_Tours_TourId",
                table: "GroupBookings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupBookings",
                table: "GroupBookings");

            migrationBuilder.RenameTable(
                name: "GroupBookings",
                newName: "GroupBooking");

            migrationBuilder.RenameIndex(
                name: "IX_GroupBookings_TourId",
                table: "GroupBooking",
                newName: "IX_GroupBooking_TourId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupBooking",
                table: "GroupBooking",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_GroupBooking_GroupBookingId",
                table: "Bookings",
                column: "GroupBookingId",
                principalTable: "GroupBooking",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupBooking_Tours_TourId",
                table: "GroupBooking",
                column: "TourId",
                principalTable: "Tours",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
