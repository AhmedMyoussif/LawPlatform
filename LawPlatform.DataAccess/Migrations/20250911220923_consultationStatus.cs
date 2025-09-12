using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LawPlatform.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class consultationStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "consultations",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "consultations");
        }
    }
}
