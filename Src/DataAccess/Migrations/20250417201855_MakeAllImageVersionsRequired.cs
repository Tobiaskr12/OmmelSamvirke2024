using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class MakeAllImageVersionsRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Images_DefaultBlobStorageFileId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_ThumbnailBlobStorageFileId",
                table: "Images");

            migrationBuilder.AlterColumn<int>(
                name: "ThumbnailBlobStorageFileId",
                table: "Images",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DefaultBlobStorageFileId",
                table: "Images",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Images_DefaultBlobStorageFileId",
                table: "Images",
                column: "DefaultBlobStorageFileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Images_ThumbnailBlobStorageFileId",
                table: "Images",
                column: "ThumbnailBlobStorageFileId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Images_DefaultBlobStorageFileId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_ThumbnailBlobStorageFileId",
                table: "Images");

            migrationBuilder.AlterColumn<int>(
                name: "ThumbnailBlobStorageFileId",
                table: "Images",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "DefaultBlobStorageFileId",
                table: "Images",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Images_DefaultBlobStorageFileId",
                table: "Images",
                column: "DefaultBlobStorageFileId",
                unique: true,
                filter: "[DefaultBlobStorageFileId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Images_ThumbnailBlobStorageFileId",
                table: "Images",
                column: "ThumbnailBlobStorageFileId",
                unique: true,
                filter: "[ThumbnailBlobStorageFileId] IS NOT NULL");
        }
    }
}
