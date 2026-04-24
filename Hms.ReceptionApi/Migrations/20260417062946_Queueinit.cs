using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hms.ReceptionApi.Migrations
{
    /// <inheritdoc />
    public partial class Queueinit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CalledAtUtc",
                table: "QueueTokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAtUtc",
                table: "QueueTokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAtUtc",
                table: "QueueTokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "QueueTokens",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SkippedAtUtc",
                table: "QueueTokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAtUtc",
                table: "QueueTokens",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CalledAtUtc",
                table: "QueueTokens");

            migrationBuilder.DropColumn(
                name: "CancelledAtUtc",
                table: "QueueTokens");

            migrationBuilder.DropColumn(
                name: "CompletedAtUtc",
                table: "QueueTokens");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "QueueTokens");

            migrationBuilder.DropColumn(
                name: "SkippedAtUtc",
                table: "QueueTokens");

            migrationBuilder.DropColumn(
                name: "StartedAtUtc",
                table: "QueueTokens");
        }
    }
}
