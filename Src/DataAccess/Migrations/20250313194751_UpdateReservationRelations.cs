using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReservationRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReservationLocationId",
                table: "Reservations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ReservationLocationId",
                table: "Reservations",
                column: "ReservationLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_ReservationLocations_ReservationLocationId",
                table: "Reservations",
                column: "ReservationLocationId",
                principalTable: "ReservationLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_ReservationLocations_ReservationLocationId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_ReservationLocationId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "ReservationLocationId",
                table: "Reservations");
        }
    }
}
