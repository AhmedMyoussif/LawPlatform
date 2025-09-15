using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LawPlatform.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class refacrot_PropsalModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OfferAmount",
                table: "Proposals",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "duration",
                table: "consultations",
                newName: "Duration");

            migrationBuilder.RenameColumn(
                name: "budget",
                table: "consultations",
                newName: "Budget");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Proposals",
                newName: "OfferAmount");

            migrationBuilder.RenameColumn(
                name: "Duration",
                table: "consultations",
                newName: "duration");

            migrationBuilder.RenameColumn(
                name: "Budget",
                table: "consultations",
                newName: "budget");
        }
    }
}
