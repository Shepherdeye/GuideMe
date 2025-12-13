using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuideMe.Migrations
{
    /// <inheritdoc />
    public partial class offerTripCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Offers_Trips_TripId",
                table: "Offers");

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_Trips_TripId",
                table: "Offers",
                column: "TripId",
                principalTable: "Trips",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Offers_Trips_TripId",
                table: "Offers");

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_Trips_TripId",
                table: "Offers",
                column: "TripId",
                principalTable: "Trips",
                principalColumn: "Id");
        }
    }
}
