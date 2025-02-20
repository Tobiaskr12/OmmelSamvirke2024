using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddRelationshiptsToNewsletterSubscriptionConfirmation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RecipientId",
                table: "NewsletterSubscriptionConfirmations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "NewsletterSubscriptionConfirmationNewsletterGroups",
                columns: table => new
                {
                    NewsletterGroupsId = table.Column<int>(type: "int", nullable: false),
                    NewsletterSubscriptionConfirmationsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsletterSubscriptionConfirmationNewsletterGroups", x => new { x.NewsletterGroupsId, x.NewsletterSubscriptionConfirmationsId });
                    table.ForeignKey(
                        name: "FK_NewsletterSubscriptionConfirmationNewsletterGroups_NewsletterGroups_NewsletterGroupsId",
                        column: x => x.NewsletterGroupsId,
                        principalTable: "NewsletterGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NewsletterSubscriptionConfirmationNewsletterGroups_NewsletterSubscriptionConfirmations_NewsletterSubscriptionConfirmationsId",
                        column: x => x.NewsletterSubscriptionConfirmationsId,
                        principalTable: "NewsletterSubscriptionConfirmations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterSubscriptionConfirmations_RecipientId",
                table: "NewsletterSubscriptionConfirmations",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterSubscriptionConfirmationNewsletterGroups_NewsletterSubscriptionConfirmationsId",
                table: "NewsletterSubscriptionConfirmationNewsletterGroups",
                column: "NewsletterSubscriptionConfirmationsId");

            migrationBuilder.AddForeignKey(
                name: "FK_NewsletterSubscriptionConfirmations_Recipients_RecipientId",
                table: "NewsletterSubscriptionConfirmations",
                column: "RecipientId",
                principalTable: "Recipients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NewsletterSubscriptionConfirmations_Recipients_RecipientId",
                table: "NewsletterSubscriptionConfirmations");

            migrationBuilder.DropTable(
                name: "NewsletterSubscriptionConfirmationNewsletterGroups");

            migrationBuilder.DropIndex(
                name: "IX_NewsletterSubscriptionConfirmations_RecipientId",
                table: "NewsletterSubscriptionConfirmations");

            migrationBuilder.DropColumn(
                name: "RecipientId",
                table: "NewsletterSubscriptionConfirmations");
        }
    }
}
