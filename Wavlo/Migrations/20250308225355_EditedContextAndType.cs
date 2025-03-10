using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wavlo.Migrations
{
    /// <inheritdoc />
    public partial class EditedContextAndType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.DropForeignKey(name: "FK_ChatUsers_Users_UserId1", table: "ChatUsers");
            migrationBuilder.DropForeignKey(name: "FK_UserImage_Users_UserId", table: "UserImage");

            
            migrationBuilder.DropIndex(name: "IX_ChatUsers_UserId1", table: "ChatUsers");

            
            migrationBuilder.DropPrimaryKey(name: "PK_ChatUsers", table: "ChatUsers");
            migrationBuilder.DropPrimaryKey(name: "PK_UserImage", table: "UserImage");

            
            migrationBuilder.DropColumn(name: "UserId1", table: "ChatUsers");

            
            migrationBuilder.RenameTable(name: "UserImage", newName: "UserImages");

            
            migrationBuilder.RenameIndex(
                name: "IX_UserImage_UserId",
                table: "UserImages",
                newName: "IX_UserImages_UserId");

            
            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "ChatUsers",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            
            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatUsers",
                table: "ChatUsers",
                columns: new[] { "ChatId", "UserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserImages",
                table: "UserImages",
                column: "Id");

            
            migrationBuilder.CreateIndex(
                name: "IX_ChatUsers_UserId",
                table: "ChatUsers",
                column: "UserId");

           
            migrationBuilder.AddForeignKey(
                name: "FK_ChatUsers_Users_UserId",
                table: "ChatUsers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            
            migrationBuilder.AddForeignKey(
                name: "FK_UserImages_Users_UserId",
                table: "UserImages",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.DropForeignKey(name: "FK_ChatUsers_Users_UserId", table: "ChatUsers");
            migrationBuilder.DropForeignKey(name: "FK_UserImages_Users_UserId", table: "UserImages");

            
            migrationBuilder.DropIndex(name: "IX_ChatUsers_UserId", table: "ChatUsers");

            
            migrationBuilder.DropPrimaryKey(name: "PK_ChatUsers", table: "ChatUsers");
            migrationBuilder.DropPrimaryKey(name: "PK_UserImages", table: "UserImages");

            
            migrationBuilder.RenameTable(name: "UserImages", newName: "UserImage");

            
            migrationBuilder.RenameIndex(
                name: "IX_UserImages_UserId",
                table: "UserImage",
                newName: "IX_UserImage_UserId");

            
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ChatUsers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            
            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "ChatUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

           
            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatUsers",
                table: "ChatUsers",
                columns: new[] { "ChatId", "UserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserImage",
                table: "UserImage",
                column: "Id");

            
            migrationBuilder.CreateIndex(
                name: "IX_ChatUsers_UserId1",
                table: "ChatUsers",
                column: "UserId1");

            
            migrationBuilder.AddForeignKey(
                name: "FK_ChatUsers_Users_UserId1",
                table: "ChatUsers",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserImage_Users_UserId",
                table: "UserImage",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
