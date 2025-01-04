using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmmelSamvirke.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SplitEmailBodyIntoTwoTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Body",
                table: "Emails",
                newName: "HtmlBody");

            migrationBuilder.AddColumn<string>(
                name: "PlainTextBody",
                table: "Emails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlainTextBody",
                table: "Emails");

            migrationBuilder.RenameColumn(
                name: "HtmlBody",
                table: "Emails",
                newName: "Body");
        }
    }
}
