using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsletterEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NewsletterGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContactListId = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsletterGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewsletterGroups_ContactLists_ContactListId",
                        column: x => x.ContactListId,
                        principalTable: "ContactLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NewsletterGroupsCleanupCampaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampaignStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CampaignDurationMonths = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsletterGroupsCleanupCampaigns", x => x.Id);
                });

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
                name: "NewsletterSubscriptionConfirmations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfirmationToken = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConfirmationExpiry = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    ConfirmationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsletterSubscriptionConfirmations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NewsletterUnsubscribeConfirmations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfirmationToken = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConfirmationExpiry = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    ConfirmationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsletterUnsubscribeConfirmations", x => x.Id);
                });

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

            migrationBuilder.CreateTable(
                name: "CampaignUncleanedRecipients",
                columns: table => new
                {
                    CampaignId = table.Column<int>(type: "int", nullable: false),
                    RecipientId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignUncleanedRecipients", x => new { x.CampaignId, x.RecipientId });
                    table.ForeignKey(
                        name: "FK_CampaignUncleanedRecipients_NewsletterGroupsCleanupCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "NewsletterGroupsCleanupCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignUncleanedRecipients_Recipients_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Recipients",
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
                name: "IX_CampaignCleanedRecipients_RecipientId",
                table: "CampaignCleanedRecipients",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignUncleanedRecipients_RecipientId",
                table: "CampaignUncleanedRecipients",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterGroups_ContactListId",
                table: "NewsletterGroups",
                column: "ContactListId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterGroups_Name",
                table: "NewsletterGroups",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterGroupsCleanupCampaigns_CampaignStart",
                table: "NewsletterGroupsCleanupCampaigns",
                column: "CampaignStart");

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

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterSubscriptionConfirmations_ConfirmationExpiry",
                table: "NewsletterSubscriptionConfirmations",
                column: "ConfirmationExpiry");

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterUnsubscribeConfirmations_ConfirmationExpiry",
                table: "NewsletterUnsubscribeConfirmations",
                column: "ConfirmationExpiry");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignCleanedRecipients");

            migrationBuilder.DropTable(
                name: "CampaignUncleanedRecipients");

            migrationBuilder.DropTable(
                name: "NewsletterNewsletterGroups");

            migrationBuilder.DropTable(
                name: "NewsletterSubscriptionConfirmations");

            migrationBuilder.DropTable(
                name: "NewsletterUnsubscribeConfirmations");

            migrationBuilder.DropTable(
                name: "NewsletterGroupsCleanupCampaigns");

            migrationBuilder.DropTable(
                name: "NewsletterGroups");

            migrationBuilder.DropTable(
                name: "Newsletters");
        }
    }
}
