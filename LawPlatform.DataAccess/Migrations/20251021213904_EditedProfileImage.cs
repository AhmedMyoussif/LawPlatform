using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LawPlatform.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class EditedProfileImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_profileImages_Clients_ClientId",
                table: "profileImages");

            migrationBuilder.DropIndex(
                name: "IX_profileImages_ClientId",
                table: "profileImages");

            migrationBuilder.AlterColumn<string>(
                name: "ClientId",
                table: "profileImages",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_profileImages_ClientId",
                table: "profileImages",
                column: "ClientId",
                unique: true,
                filter: "[ClientId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_profileImages_Clients_ClientId",
                table: "profileImages",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_profileImages_Clients_ClientId",
                table: "profileImages");

            migrationBuilder.DropIndex(
                name: "IX_profileImages_ClientId",
                table: "profileImages");

            migrationBuilder.AlterColumn<string>(
                name: "ClientId",
                table: "profileImages",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_profileImages_ClientId",
                table: "profileImages",
                column: "ClientId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_profileImages_Clients_ClientId",
                table: "profileImages",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
