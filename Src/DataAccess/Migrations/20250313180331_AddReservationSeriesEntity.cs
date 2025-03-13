using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationSeriesEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReservationSeriesId",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReservationSeries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecurrenceType = table.Column<int>(type: "int", nullable: false),
                    Interval = table.Column<int>(type: "int", nullable: false),
                    RecurrenceStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RecurrenceEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationSeries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ReservationSeriesId",
                table: "Reservations",
                column: "ReservationSeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationSeries_RecurrenceEndDate",
                table: "ReservationSeries",
                column: "RecurrenceEndDate");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationSeries_RecurrenceStartDate",
                table: "ReservationSeries",
                column: "RecurrenceStartDate");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_ReservationSeries_ReservationSeriesId",
                table: "Reservations",
                column: "ReservationSeriesId",
                principalTable: "ReservationSeries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_ReservationSeries_ReservationSeriesId",
                table: "Reservations");

            migrationBuilder.DropTable(
                name: "ReservationSeries");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_ReservationSeriesId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "ReservationSeriesId",
                table: "Reservations");
        }
    }
}
