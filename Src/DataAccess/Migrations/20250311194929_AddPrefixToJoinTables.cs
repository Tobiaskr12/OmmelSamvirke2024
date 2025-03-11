using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddPrefixToJoinTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CampaignUncleanedRecipients_NewsletterGroupsCleanupCampaigns_CampaignId",
                table: "CampaignUncleanedRecipients");

            migrationBuilder.DropForeignKey(
                name: "FK_CampaignUncleanedRecipients_Recipients_RecipientId",
                table: "CampaignUncleanedRecipients");

            migrationBuilder.DropForeignKey(
                name: "FK_ContactListRecipients_ContactLists_ContactListId",
                table: "ContactListRecipients");

            migrationBuilder.DropForeignKey(
                name: "FK_ContactListRecipients_Recipients_RecipientId",
                table: "ContactListRecipients");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailRecipients_Emails_EmailId",
                table: "EmailRecipients");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailRecipients_Recipients_RecipientId",
                table: "EmailRecipients");

            migrationBuilder.DropForeignKey(
                name: "FK_NewsletterSubscriptionConfirmationNewsletterGroups_NewsletterGroups_NewsletterGroupsId",
                table: "NewsletterSubscriptionConfirmationNewsletterGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_NewsletterSubscriptionConfirmationNewsletterGroups_NewsletterSubscriptionConfirmations_NewsletterSubscriptionConfirmationsId",
                table: "NewsletterSubscriptionConfirmationNewsletterGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_NewsletterUnsubscribeConfirmationNewsletterGroups_NewsletterGroups_NewsletterGroupsId",
                table: "NewsletterUnsubscribeConfirmationNewsletterGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_NewsletterUnsubscribeConfirmationNewsletterGroups_NewsletterUnsubscribeConfirmations_NewsletterUnsubscribeConfirmationsId",
                table: "NewsletterUnsubscribeConfirmationNewsletterGroups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NewsletterUnsubscribeConfirmationNewsletterGroups",
                table: "NewsletterUnsubscribeConfirmationNewsletterGroups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NewsletterSubscriptionConfirmationNewsletterGroups",
                table: "NewsletterSubscriptionConfirmationNewsletterGroups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmailRecipients",
                table: "EmailRecipients");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContactListRecipients",
                table: "ContactListRecipients");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CampaignUncleanedRecipients",
                table: "CampaignUncleanedRecipients");

            migrationBuilder.RenameTable(
                name: "NewsletterUnsubscribeConfirmationNewsletterGroups",
                newName: "Join_NewsletterUnsubscribeConfirmationNewsletterGroups");

            migrationBuilder.RenameTable(
                name: "NewsletterSubscriptionConfirmationNewsletterGroups",
                newName: "Join_NewsletterSubscriptionConfirmationNewsletterGroups");

            migrationBuilder.RenameTable(
                name: "EmailRecipients",
                newName: "Join_EmailRecipients");

            migrationBuilder.RenameTable(
                name: "ContactListRecipients",
                newName: "Join_ContactListRecipients");

            migrationBuilder.RenameTable(
                name: "CampaignUncleanedRecipients",
                newName: "Join_CampaignRecipients");

            migrationBuilder.RenameIndex(
                name: "IX_NewsletterUnsubscribeConfirmationNewsletterGroups_NewsletterUnsubscribeConfirmationsId",
                table: "Join_NewsletterUnsubscribeConfirmationNewsletterGroups",
                newName: "IX_Join_NewsletterUnsubscribeConfirmationNewsletterGroups_NewsletterUnsubscribeConfirmationsId");

            migrationBuilder.RenameIndex(
                name: "IX_NewsletterSubscriptionConfirmationNewsletterGroups_NewsletterSubscriptionConfirmationsId",
                table: "Join_NewsletterSubscriptionConfirmationNewsletterGroups",
                newName: "IX_Join_NewsletterSubscriptionConfirmationNewsletterGroups_NewsletterSubscriptionConfirmationsId");

            migrationBuilder.RenameIndex(
                name: "IX_EmailRecipients_RecipientId",
                table: "Join_EmailRecipients",
                newName: "IX_Join_EmailRecipients_RecipientId");

            migrationBuilder.RenameIndex(
                name: "IX_ContactListRecipients_RecipientId",
                table: "Join_ContactListRecipients",
                newName: "IX_Join_ContactListRecipients_RecipientId");

            migrationBuilder.RenameIndex(
                name: "IX_CampaignUncleanedRecipients_RecipientId",
                table: "Join_CampaignRecipients",
                newName: "IX_Join_CampaignRecipients_RecipientId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Join_NewsletterUnsubscribeConfirmationNewsletterGroups",
                table: "Join_NewsletterUnsubscribeConfirmationNewsletterGroups",
                columns: new[] { "NewsletterGroupsId", "NewsletterUnsubscribeConfirmationsId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Join_NewsletterSubscriptionConfirmationNewsletterGroups",
                table: "Join_NewsletterSubscriptionConfirmationNewsletterGroups",
                columns: new[] { "NewsletterGroupsId", "NewsletterSubscriptionConfirmationsId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Join_EmailRecipients",
                table: "Join_EmailRecipients",
                columns: new[] { "EmailId", "RecipientId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Join_ContactListRecipients",
                table: "Join_ContactListRecipients",
                columns: new[] { "ContactListId", "RecipientId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Join_CampaignRecipients",
                table: "Join_CampaignRecipients",
                columns: new[] { "CampaignId", "RecipientId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Join_CampaignRecipients_NewsletterGroupsCleanupCampaigns_CampaignId",
                table: "Join_CampaignRecipients",
                column: "CampaignId",
                principalTable: "NewsletterGroupsCleanupCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Join_CampaignRecipients_Recipients_RecipientId",
                table: "Join_CampaignRecipients",
                column: "RecipientId",
                principalTable: "Recipients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Join_ContactListRecipients_ContactLists_ContactListId",
                table: "Join_ContactListRecipients",
                column: "ContactListId",
                principalTable: "ContactLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Join_ContactListRecipients_Recipients_RecipientId",
                table: "Join_ContactListRecipients",
                column: "RecipientId",
                principalTable: "Recipients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Join_EmailRecipients_Emails_EmailId",
                table: "Join_EmailRecipients",
                column: "EmailId",
                principalTable: "Emails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Join_EmailRecipients_Recipients_RecipientId",
                table: "Join_EmailRecipients",
                column: "RecipientId",
                principalTable: "Recipients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Join_NewsletterSubscriptionConfirmationNewsletterGroups_NewsletterGroups_NewsletterGroupsId",
                table: "Join_NewsletterSubscriptionConfirmationNewsletterGroups",
                column: "NewsletterGroupsId",
                principalTable: "NewsletterGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Join_NewsletterSubscriptionConfirmationNewsletterGroups_NewsletterSubscriptionConfirmations_NewsletterSubscriptionConfirmati~",
                table: "Join_NewsletterSubscriptionConfirmationNewsletterGroups",
                column: "NewsletterSubscriptionConfirmationsId",
                principalTable: "NewsletterSubscriptionConfirmations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Join_NewsletterUnsubscribeConfirmationNewsletterGroups_NewsletterGroups_NewsletterGroupsId",
                table: "Join_NewsletterUnsubscribeConfirmationNewsletterGroups",
                column: "NewsletterGroupsId",
                principalTable: "NewsletterGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Join_NewsletterUnsubscribeConfirmationNewsletterGroups_NewsletterUnsubscribeConfirmations_NewsletterUnsubscribeConfirmations~",
                table: "Join_NewsletterUnsubscribeConfirmationNewsletterGroups",
                column: "NewsletterUnsubscribeConfirmationsId",
                principalTable: "NewsletterUnsubscribeConfirmations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Join_CampaignRecipients_NewsletterGroupsCleanupCampaigns_CampaignId",
                table: "Join_CampaignRecipients");

            migrationBuilder.DropForeignKey(
                name: "FK_Join_CampaignRecipients_Recipients_RecipientId",
                table: "Join_CampaignRecipients");

            migrationBuilder.DropForeignKey(
                name: "FK_Join_ContactListRecipients_ContactLists_ContactListId",
                table: "Join_ContactListRecipients");

            migrationBuilder.DropForeignKey(
                name: "FK_Join_ContactListRecipients_Recipients_RecipientId",
                table: "Join_ContactListRecipients");

            migrationBuilder.DropForeignKey(
                name: "FK_Join_EmailRecipients_Emails_EmailId",
                table: "Join_EmailRecipients");

            migrationBuilder.DropForeignKey(
                name: "FK_Join_EmailRecipients_Recipients_RecipientId",
                table: "Join_EmailRecipients");

            migrationBuilder.DropForeignKey(
                name: "FK_Join_NewsletterSubscriptionConfirmationNewsletterGroups_NewsletterGroups_NewsletterGroupsId",
                table: "Join_NewsletterSubscriptionConfirmationNewsletterGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_Join_NewsletterSubscriptionConfirmationNewsletterGroups_NewsletterSubscriptionConfirmations_NewsletterSubscriptionConfirmati~",
                table: "Join_NewsletterSubscriptionConfirmationNewsletterGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_Join_NewsletterUnsubscribeConfirmationNewsletterGroups_NewsletterGroups_NewsletterGroupsId",
                table: "Join_NewsletterUnsubscribeConfirmationNewsletterGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_Join_NewsletterUnsubscribeConfirmationNewsletterGroups_NewsletterUnsubscribeConfirmations_NewsletterUnsubscribeConfirmations~",
                table: "Join_NewsletterUnsubscribeConfirmationNewsletterGroups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Join_NewsletterUnsubscribeConfirmationNewsletterGroups",
                table: "Join_NewsletterUnsubscribeConfirmationNewsletterGroups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Join_NewsletterSubscriptionConfirmationNewsletterGroups",
                table: "Join_NewsletterSubscriptionConfirmationNewsletterGroups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Join_EmailRecipients",
                table: "Join_EmailRecipients");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Join_ContactListRecipients",
                table: "Join_ContactListRecipients");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Join_CampaignRecipients",
                table: "Join_CampaignRecipients");

            migrationBuilder.RenameTable(
                name: "Join_NewsletterUnsubscribeConfirmationNewsletterGroups",
                newName: "NewsletterUnsubscribeConfirmationNewsletterGroups");

            migrationBuilder.RenameTable(
                name: "Join_NewsletterSubscriptionConfirmationNewsletterGroups",
                newName: "NewsletterSubscriptionConfirmationNewsletterGroups");

            migrationBuilder.RenameTable(
                name: "Join_EmailRecipients",
                newName: "EmailRecipients");

            migrationBuilder.RenameTable(
                name: "Join_ContactListRecipients",
                newName: "ContactListRecipients");

            migrationBuilder.RenameTable(
                name: "Join_CampaignRecipients",
                newName: "CampaignUncleanedRecipients");

            migrationBuilder.RenameIndex(
                name: "IX_Join_NewsletterUnsubscribeConfirmationNewsletterGroups_NewsletterUnsubscribeConfirmationsId",
                table: "NewsletterUnsubscribeConfirmationNewsletterGroups",
                newName: "IX_NewsletterUnsubscribeConfirmationNewsletterGroups_NewsletterUnsubscribeConfirmationsId");

            migrationBuilder.RenameIndex(
                name: "IX_Join_NewsletterSubscriptionConfirmationNewsletterGroups_NewsletterSubscriptionConfirmationsId",
                table: "NewsletterSubscriptionConfirmationNewsletterGroups",
                newName: "IX_NewsletterSubscriptionConfirmationNewsletterGroups_NewsletterSubscriptionConfirmationsId");

            migrationBuilder.RenameIndex(
                name: "IX_Join_EmailRecipients_RecipientId",
                table: "EmailRecipients",
                newName: "IX_EmailRecipients_RecipientId");

            migrationBuilder.RenameIndex(
                name: "IX_Join_ContactListRecipients_RecipientId",
                table: "ContactListRecipients",
                newName: "IX_ContactListRecipients_RecipientId");

            migrationBuilder.RenameIndex(
                name: "IX_Join_CampaignRecipients_RecipientId",
                table: "CampaignUncleanedRecipients",
                newName: "IX_CampaignUncleanedRecipients_RecipientId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NewsletterUnsubscribeConfirmationNewsletterGroups",
                table: "NewsletterUnsubscribeConfirmationNewsletterGroups",
                columns: new[] { "NewsletterGroupsId", "NewsletterUnsubscribeConfirmationsId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_NewsletterSubscriptionConfirmationNewsletterGroups",
                table: "NewsletterSubscriptionConfirmationNewsletterGroups",
                columns: new[] { "NewsletterGroupsId", "NewsletterSubscriptionConfirmationsId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmailRecipients",
                table: "EmailRecipients",
                columns: new[] { "EmailId", "RecipientId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContactListRecipients",
                table: "ContactListRecipients",
                columns: new[] { "ContactListId", "RecipientId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CampaignUncleanedRecipients",
                table: "CampaignUncleanedRecipients",
                columns: new[] { "CampaignId", "RecipientId" });

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignUncleanedRecipients_NewsletterGroupsCleanupCampaigns_CampaignId",
                table: "CampaignUncleanedRecipients",
                column: "CampaignId",
                principalTable: "NewsletterGroupsCleanupCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignUncleanedRecipients_Recipients_RecipientId",
                table: "CampaignUncleanedRecipients",
                column: "RecipientId",
                principalTable: "Recipients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ContactListRecipients_ContactLists_ContactListId",
                table: "ContactListRecipients",
                column: "ContactListId",
                principalTable: "ContactLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ContactListRecipients_Recipients_RecipientId",
                table: "ContactListRecipients",
                column: "RecipientId",
                principalTable: "Recipients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailRecipients_Emails_EmailId",
                table: "EmailRecipients",
                column: "EmailId",
                principalTable: "Emails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailRecipients_Recipients_RecipientId",
                table: "EmailRecipients",
                column: "RecipientId",
                principalTable: "Recipients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NewsletterSubscriptionConfirmationNewsletterGroups_NewsletterGroups_NewsletterGroupsId",
                table: "NewsletterSubscriptionConfirmationNewsletterGroups",
                column: "NewsletterGroupsId",
                principalTable: "NewsletterGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NewsletterSubscriptionConfirmationNewsletterGroups_NewsletterSubscriptionConfirmations_NewsletterSubscriptionConfirmationsId",
                table: "NewsletterSubscriptionConfirmationNewsletterGroups",
                column: "NewsletterSubscriptionConfirmationsId",
                principalTable: "NewsletterSubscriptionConfirmations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NewsletterUnsubscribeConfirmationNewsletterGroups_NewsletterGroups_NewsletterGroupsId",
                table: "NewsletterUnsubscribeConfirmationNewsletterGroups",
                column: "NewsletterGroupsId",
                principalTable: "NewsletterGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NewsletterUnsubscribeConfirmationNewsletterGroups_NewsletterUnsubscribeConfirmations_NewsletterUnsubscribeConfirmationsId",
                table: "NewsletterUnsubscribeConfirmationNewsletterGroups",
                column: "NewsletterUnsubscribeConfirmationsId",
                principalTable: "NewsletterUnsubscribeConfirmations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
