using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LawPlatform.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RelationbetwwenUsers_Picture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_profileImages_AspNetUsers_UserId",
                table: "profileImages");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "profileImages",
                newName: "ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_profileImages_UserId",
                table: "profileImages",
                newName: "IX_profileImages_ClientId");

            migrationBuilder.AddColumn<string>(
                name: "LawyerId",
                table: "profileImages",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_profileImages_LawyerId",
                table: "profileImages",
                column: "LawyerId");

            migrationBuilder.AddForeignKey(
                name: "FK_profileImages_Clients_ClientId",
                table: "profileImages",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_profileImages_Lawyers_LawyerId",
                table: "profileImages",
                column: "LawyerId",
                principalTable: "Lawyers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_profileImages_Clients_ClientId",
                table: "profileImages");

            migrationBuilder.DropForeignKey(
                name: "FK_profileImages_Lawyers_LawyerId",
                table: "profileImages");

            migrationBuilder.DropIndex(
                name: "IX_profileImages_LawyerId",
                table: "profileImages");

            migrationBuilder.DropColumn(
                name: "LawyerId",
                table: "profileImages");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "profileImages",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_profileImages_ClientId",
                table: "profileImages",
                newName: "IX_profileImages_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_profileImages_AspNetUsers_UserId",
                table: "profileImages",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
