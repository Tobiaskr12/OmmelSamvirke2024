using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class CursedMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Token",
                table: "Recipients",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastReminderSent",
                table: "NewsletterGroupsCleanupCampaigns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recipients_Token",
                table: "Recipients",
                column: "Token");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Recipients_Token",
                table: "Recipients");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "Recipients");

            migrationBuilder.DropColumn(
                name: "LastReminderSent",
                table: "NewsletterGroupsCleanupCampaigns");
        }
    }
}
