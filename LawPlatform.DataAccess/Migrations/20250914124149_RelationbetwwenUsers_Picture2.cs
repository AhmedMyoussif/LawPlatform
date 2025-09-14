using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LawPlatform.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RelationbetwwenUsers_Picture2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_profileImages_ClientId",
                table: "profileImages");

            migrationBuilder.DropIndex(
                name: "IX_profileImages_LawyerId",
                table: "profileImages");

            migrationBuilder.CreateIndex(
                name: "IX_profileImages_ClientId",
                table: "profileImages",
                column: "ClientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_profileImages_LawyerId",
                table: "profileImages",
                column: "LawyerId",
                unique: true,
                filter: "[LawyerId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_profileImages_ClientId",
                table: "profileImages");

            migrationBuilder.DropIndex(
                name: "IX_profileImages_LawyerId",
                table: "profileImages");

            migrationBuilder.CreateIndex(
                name: "IX_profileImages_ClientId",
                table: "profileImages",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_profileImages_LawyerId",
                table: "profileImages",
                column: "LawyerId");
        }
    }
}
