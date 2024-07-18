using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dotSocialNetwork.Migrations
{
    /// <inheritdoc />
    public partial class Initial5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Sender",
                table: "Message",
                newName: "SenderId");

            migrationBuilder.RenameColumn(
                name: "Recipient",
                table: "Message",
                newName: "RecipientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SenderId",
                table: "Message",
                newName: "Sender");

            migrationBuilder.RenameColumn(
                name: "RecipientId",
                table: "Message",
                newName: "Recipient");
        }
    }
}
