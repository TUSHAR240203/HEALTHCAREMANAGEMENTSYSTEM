using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hms.DoctorsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthUserIdToDoctors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AuthUserId",
                table: "Doctors",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_AuthUserId",
                table: "Doctors",
                column: "AuthUserId",
                unique: true,
                filter: "[AuthUserId] IS NOT NULL");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "DoctorLeaves",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAtUtc",
                table: "DoctorLeaves",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewedBy",
                table: "DoctorLeaves",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewedBy",
                table: "DoctorLeaves");

            migrationBuilder.DropColumn(
                name: "ReviewedAtUtc",
                table: "DoctorLeaves");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "DoctorLeaves");

            migrationBuilder.DropIndex(
                name: "IX_Doctors_AuthUserId",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "AuthUserId",
                table: "Doctors");
        }
    }
}