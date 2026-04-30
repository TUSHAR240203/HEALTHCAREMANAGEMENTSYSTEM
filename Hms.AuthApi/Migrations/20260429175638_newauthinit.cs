using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hms.AuthApi.Migrations
{
    /// <inheritdoc />
    public partial class newauthinit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 4, 29, 17, 56, 38, 452, DateTimeKind.Utc).AddTicks(1545));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 4, 29, 17, 56, 38, 452, DateTimeKind.Utc).AddTicks(1592));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 4, 29, 17, 56, 38, 452, DateTimeKind.Utc).AddTicks(1594));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 4, 29, 17, 56, 38, 452, DateTimeKind.Utc).AddTicks(1596));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 4, 28, 9, 14, 23, 975, DateTimeKind.Utc).AddTicks(8386));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 4, 28, 9, 14, 23, 975, DateTimeKind.Utc).AddTicks(8430));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 4, 28, 9, 14, 23, 975, DateTimeKind.Utc).AddTicks(8433));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 4, 28, 9, 14, 23, 975, DateTimeKind.Utc).AddTicks(8436));
        }
    }
}
