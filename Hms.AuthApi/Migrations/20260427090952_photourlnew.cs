using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hms.AuthApi.Migrations
{
    /// <inheritdoc />
    public partial class photourlnew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_MobileNumber",
                table: "Users");

            migrationBuilder.AddColumn<bool>(
                name: "IsFirstLoginCompleted",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOtpLoginEnabled",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPasswordLoginEnabled",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LoginId",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 4, 27, 9, 9, 51, 350, DateTimeKind.Utc).AddTicks(8937));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 4, 27, 9, 9, 51, 350, DateTimeKind.Utc).AddTicks(9089));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 4, 27, 9, 9, 51, 350, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 4, 27, 9, 9, 51, 350, DateTimeKind.Utc).AddTicks(9101));

            migrationBuilder.CreateIndex(
                name: "IX_Users_LoginId",
                table: "Users",
                column: "LoginId",
                unique: true,
                filter: "[LoginId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_MobileNumber",
                table: "Users",
                column: "MobileNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_LoginId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_MobileNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsFirstLoginCompleted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsOtpLoginEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsPasswordLoginEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LoginId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 4, 22, 7, 9, 23, 731, DateTimeKind.Utc).AddTicks(3029));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 4, 22, 7, 9, 23, 731, DateTimeKind.Utc).AddTicks(3086));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 4, 22, 7, 9, 23, 731, DateTimeKind.Utc).AddTicks(3090));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 4, 22, 7, 9, 23, 731, DateTimeKind.Utc).AddTicks(3094));

            migrationBuilder.CreateIndex(
                name: "IX_Users_MobileNumber",
                table: "Users",
                column: "MobileNumber",
                unique: true);
        }
    }
}
