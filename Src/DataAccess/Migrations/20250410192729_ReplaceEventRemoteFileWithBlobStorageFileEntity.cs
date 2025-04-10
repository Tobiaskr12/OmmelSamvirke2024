using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceEventRemoteFileWithBlobStorageFileEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventRemoteFiles");

            migrationBuilder.AddColumn<int>(
                name: "EventId",
                table: "BlobStorageFiles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlobStorageFiles_EventId",
                table: "BlobStorageFiles",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_BlobStorageFiles_Events_EventId",
                table: "BlobStorageFiles",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlobStorageFiles_Events_EventId",
                table: "BlobStorageFiles");

            migrationBuilder.DropIndex(
                name: "IX_BlobStorageFiles_EventId",
                table: "BlobStorageFiles");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "BlobStorageFiles");

            migrationBuilder.CreateTable(
                name: "EventRemoteFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EventId = table.Column<int>(type: "int", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventRemoteFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventRemoteFiles_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventRemoteFiles_EventId",
                table: "EventRemoteFiles",
                column: "EventId");
        }
    }
}
