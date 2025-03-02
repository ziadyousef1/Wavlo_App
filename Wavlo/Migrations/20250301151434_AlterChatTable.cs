using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wavlo.Migrations
{
    /// <inheritdoc />
    public partial class AlterChatTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Chats",
                newName: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Chats",
                newName: "UserId");
        }
    }
}
