using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LawPlatform.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLawyerData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lawyers_AspNetUsers_Id",
                table: "Lawyers");

            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "Lawyers");

            migrationBuilder.AlterColumn<string>(
                name: "Experiences",
                table: "Lawyers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "BankName",
                table: "Lawyers",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "IBAN",
                table: "Lawyers",
                type: "nvarchar(34)",
                maxLength: 34,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LicenseDocumentPath",
                table: "Lawyers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LicenseNumber",
                table: "Lawyers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Specialization",
                table: "Lawyers",
                type: "int",
                maxLength: 100,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Lawyers",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Lawyers_UserId",
                table: "Lawyers",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Lawyers_AspNetUsers_UserId",
                table: "Lawyers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lawyers_AspNetUsers_UserId",
                table: "Lawyers");

            migrationBuilder.DropIndex(
                name: "IX_Lawyers_UserId",
                table: "Lawyers");

            migrationBuilder.DropColumn(
                name: "IBAN",
                table: "Lawyers");

            migrationBuilder.DropColumn(
                name: "LicenseDocumentPath",
                table: "Lawyers");

            migrationBuilder.DropColumn(
                name: "LicenseNumber",
                table: "Lawyers");

            migrationBuilder.DropColumn(
                name: "Specialization",
                table: "Lawyers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Lawyers");

            migrationBuilder.AlterColumn<string>(
                name: "Experiences",
                table: "Lawyers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "BankName",
                table: "Lawyers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AddColumn<DateTime>(
                name: "BirthDate",
                table: "Lawyers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_Lawyers_AspNetUsers_Id",
                table: "Lawyers",
                column: "Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
