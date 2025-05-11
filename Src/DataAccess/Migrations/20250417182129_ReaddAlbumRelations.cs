using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ReaddAlbumRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_BlobStorageFiles_OriginalBlobStorageFileId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_DefaultBlobStorageFileId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_OriginalBlobStorageFileId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_ThumbnailBlobStorageFileId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Albums_CoverImageId",
                table: "Albums");

            migrationBuilder.CreateIndex(
                name: "IX_Images_DefaultBlobStorageFileId",
                table: "Images",
                column: "DefaultBlobStorageFileId",
                unique: true,
                filter: "[DefaultBlobStorageFileId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Images_OriginalBlobStorageFileId",
                table: "Images",
                column: "OriginalBlobStorageFileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Images_ThumbnailBlobStorageFileId",
                table: "Images",
                column: "ThumbnailBlobStorageFileId",
                unique: true,
                filter: "[ThumbnailBlobStorageFileId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_CoverImageId",
                table: "Albums",
                column: "CoverImageId",
                unique: true,
                filter: "[CoverImageId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_BlobStorageFiles_OriginalBlobStorageFileId",
                table: "Images",
                column: "OriginalBlobStorageFileId",
                principalTable: "BlobStorageFiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_BlobStorageFiles_OriginalBlobStorageFileId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_DefaultBlobStorageFileId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_OriginalBlobStorageFileId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_ThumbnailBlobStorageFileId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Albums_CoverImageId",
                table: "Albums");

            migrationBuilder.CreateIndex(
                name: "IX_Images_DefaultBlobStorageFileId",
                table: "Images",
                column: "DefaultBlobStorageFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Images_OriginalBlobStorageFileId",
                table: "Images",
                column: "OriginalBlobStorageFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Images_ThumbnailBlobStorageFileId",
                table: "Images",
                column: "ThumbnailBlobStorageFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_CoverImageId",
                table: "Albums",
                column: "CoverImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_BlobStorageFiles_OriginalBlobStorageFileId",
                table: "Images",
                column: "OriginalBlobStorageFileId",
                principalTable: "BlobStorageFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
