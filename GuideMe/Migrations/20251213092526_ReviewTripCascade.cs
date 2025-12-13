using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuideMe.Migrations
{
    /// <inheritdoc />
    public partial class ReviewTripCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Offers_Guides_GuideId",
                table: "Offers");

            migrationBuilder.DropForeignKey(
                name: "FK_Offers_Trips_TripId",
                table: "Offers");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Trips_TripId",
                table: "Reviews");

            migrationBuilder.AlterColumn<int>(
                name: "TripId",
                table: "Offers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "GuideId",
                table: "Offers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_Guides_GuideId",
                table: "Offers",
                column: "GuideId",
                principalTable: "Guides",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_Trips_TripId",
                table: "Offers",
                column: "TripId",
                principalTable: "Trips",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Trips_TripId",
                table: "Reviews",
                column: "TripId",
                principalTable: "Trips",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Offers_Guides_GuideId",
                table: "Offers");

            migrationBuilder.DropForeignKey(
                name: "FK_Offers_Trips_TripId",
                table: "Offers");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Trips_TripId",
                table: "Reviews");

            migrationBuilder.AlterColumn<int>(
                name: "TripId",
                table: "Offers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GuideId",
                table: "Offers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_Guides_GuideId",
                table: "Offers",
                column: "GuideId",
                principalTable: "Guides",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_Trips_TripId",
                table: "Offers",
                column: "TripId",
                principalTable: "Trips",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Trips_TripId",
                table: "Reviews",
                column: "TripId",
                principalTable: "Trips",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
