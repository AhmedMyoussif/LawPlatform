using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LawPlatform.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class drop_category : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_consultations_ConsultationCategories_CategoryId",
                table: "consultations");

            migrationBuilder.DropTable(
                name: "ConsultationCategories");

            migrationBuilder.DropIndex(
                name: "IX_consultations_CategoryId",
                table: "consultations");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "consultations");

            migrationBuilder.AddColumn<int>(
                name: "Specialization",
                table: "consultations",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Specialization",
                table: "consultations");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "consultations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ConsultationCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_consultations_CategoryId",
                table: "consultations",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_consultations_ConsultationCategories_CategoryId",
                table: "consultations",
                column: "CategoryId",
                principalTable: "ConsultationCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
