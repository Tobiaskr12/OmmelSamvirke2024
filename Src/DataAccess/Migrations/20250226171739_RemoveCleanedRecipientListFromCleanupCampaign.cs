using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCleanedRecipientListFromCleanupCampaign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignCleanedRecipients");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CampaignCleanedRecipients",
                columns: table => new
                {
                    CampaignId = table.Column<int>(type: "int", nullable: false),
                    RecipientId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignCleanedRecipients", x => new { x.CampaignId, x.RecipientId });
                    table.ForeignKey(
                        name: "FK_CampaignCleanedRecipients_NewsletterGroupsCleanupCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "NewsletterGroupsCleanupCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignCleanedRecipients_Recipients_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Recipients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignCleanedRecipients_RecipientId",
                table: "CampaignCleanedRecipients",
                column: "RecipientId");
        }
    }
}
