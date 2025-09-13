using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LawPlatform.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Add_Column_LawyerId_For_Condultation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LawyerId",
                table: "consultations",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_consultations_LawyerId",
                table: "consultations",
                column: "LawyerId");

            migrationBuilder.AddForeignKey(
                name: "FK_consultations_Lawyers_LawyerId",
                table: "consultations",
                column: "LawyerId",
                principalTable: "Lawyers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_consultations_Lawyers_LawyerId",
                table: "consultations");

            migrationBuilder.DropIndex(
                name: "IX_consultations_LawyerId",
                table: "consultations");

            migrationBuilder.DropColumn(
                name: "LawyerId",
                table: "consultations");
        }
    }
}
