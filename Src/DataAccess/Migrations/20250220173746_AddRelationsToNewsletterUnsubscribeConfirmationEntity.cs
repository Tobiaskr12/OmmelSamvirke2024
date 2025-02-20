using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddRelationsToNewsletterUnsubscribeConfirmationEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RecipientId",
                table: "NewsletterUnsubscribeConfirmations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "NewsletterUnsubscribeConfirmationNewsletterGroups",
                columns: table => new
                {
                    NewsletterGroupsId = table.Column<int>(type: "int", nullable: false),
                    NewsletterUnsubscribeConfirmationsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsletterUnsubscribeConfirmationNewsletterGroups", x => new { x.NewsletterGroupsId, x.NewsletterUnsubscribeConfirmationsId });
                    table.ForeignKey(
                        name: "FK_NewsletterUnsubscribeConfirmationNewsletterGroups_NewsletterGroups_NewsletterGroupsId",
                        column: x => x.NewsletterGroupsId,
                        principalTable: "NewsletterGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NewsletterUnsubscribeConfirmationNewsletterGroups_NewsletterUnsubscribeConfirmations_NewsletterUnsubscribeConfirmationsId",
                        column: x => x.NewsletterUnsubscribeConfirmationsId,
                        principalTable: "NewsletterUnsubscribeConfirmations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterUnsubscribeConfirmations_RecipientId",
                table: "NewsletterUnsubscribeConfirmations",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterUnsubscribeConfirmationNewsletterGroups_NewsletterUnsubscribeConfirmationsId",
                table: "NewsletterUnsubscribeConfirmationNewsletterGroups",
                column: "NewsletterUnsubscribeConfirmationsId");

            migrationBuilder.AddForeignKey(
                name: "FK_NewsletterUnsubscribeConfirmations_Recipients_RecipientId",
                table: "NewsletterUnsubscribeConfirmations",
                column: "RecipientId",
                principalTable: "Recipients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NewsletterUnsubscribeConfirmations_Recipients_RecipientId",
                table: "NewsletterUnsubscribeConfirmations");

            migrationBuilder.DropTable(
                name: "NewsletterUnsubscribeConfirmationNewsletterGroups");

            migrationBuilder.DropIndex(
                name: "IX_NewsletterUnsubscribeConfirmations_RecipientId",
                table: "NewsletterUnsubscribeConfirmations");

            migrationBuilder.DropColumn(
                name: "RecipientId",
                table: "NewsletterUnsubscribeConfirmations");
        }
    }
}
