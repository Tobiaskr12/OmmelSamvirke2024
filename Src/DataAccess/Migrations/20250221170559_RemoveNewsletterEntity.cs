using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNewsletterEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewsletterNewsletterGroups");

            migrationBuilder.DropTable(
                name: "Newsletters");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Newsletters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmailId = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Newsletters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Newsletters_Emails_EmailId",
                        column: x => x.EmailId,
                        principalTable: "Emails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NewsletterNewsletterGroups",
                columns: table => new
                {
                    NewsletterId = table.Column<int>(type: "int", nullable: false),
                    NewsletterGroupId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsletterNewsletterGroups", x => new { x.NewsletterId, x.NewsletterGroupId });
                    table.ForeignKey(
                        name: "FK_NewsletterNewsletterGroups_NewsletterGroups_NewsletterGroupId",
                        column: x => x.NewsletterGroupId,
                        principalTable: "NewsletterGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NewsletterNewsletterGroups_Newsletters_NewsletterId",
                        column: x => x.NewsletterId,
                        principalTable: "Newsletters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterNewsletterGroups_NewsletterGroupId",
                table: "NewsletterNewsletterGroups",
                column: "NewsletterGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Newsletters_DateCreated",
                table: "Newsletters",
                column: "DateCreated");

            migrationBuilder.CreateIndex(
                name: "IX_Newsletters_DateModified",
                table: "Newsletters",
                column: "DateModified");

            migrationBuilder.CreateIndex(
                name: "IX_Newsletters_EmailId",
                table: "Newsletters",
                column: "EmailId",
                unique: true);
        }
    }
}
