using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Base.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexOnEmailDateCreated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Emails_DateCreated",
                table: "Emails",
                column: "DateCreated");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Emails_DateCreated",
                table: "Emails");
        }
    }
}
