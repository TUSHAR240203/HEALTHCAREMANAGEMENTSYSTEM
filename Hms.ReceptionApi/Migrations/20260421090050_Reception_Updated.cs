using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hms.ReceptionApi.Migrations
{
    /// <inheritdoc />
    public partial class Reception_Updated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PatientName",
                table: "QueueTokens",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "QueueTokens",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "QueueTokens",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "PatientCheckIns",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "PatientCheckIns",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_QueueTokens_AppointmentId",
                table: "QueueTokens",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_QueueTokens_DepartmentId_QueueDate_Status",
                table: "QueueTokens",
                columns: new[] { "DepartmentId", "QueueDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_QueueTokens_DoctorId",
                table: "QueueTokens",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_QueueTokens_PatientId",
                table: "QueueTokens",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientCheckIns_DepartmentId",
                table: "PatientCheckIns",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientCheckIns_DoctorId",
                table: "PatientCheckIns",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientCheckIns_PatientId_AppointmentId",
                table: "PatientCheckIns",
                columns: new[] { "PatientId", "AppointmentId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QueueTokens_AppointmentId",
                table: "QueueTokens");

            migrationBuilder.DropIndex(
                name: "IX_QueueTokens_DepartmentId_QueueDate_Status",
                table: "QueueTokens");

            migrationBuilder.DropIndex(
                name: "IX_QueueTokens_DoctorId",
                table: "QueueTokens");

            migrationBuilder.DropIndex(
                name: "IX_QueueTokens_PatientId",
                table: "QueueTokens");

            migrationBuilder.DropIndex(
                name: "IX_PatientCheckIns_DepartmentId",
                table: "PatientCheckIns");

            migrationBuilder.DropIndex(
                name: "IX_PatientCheckIns_DoctorId",
                table: "PatientCheckIns");

            migrationBuilder.DropIndex(
                name: "IX_PatientCheckIns_PatientId_AppointmentId",
                table: "PatientCheckIns");

            migrationBuilder.AlterColumn<string>(
                name: "PatientName",
                table: "QueueTokens",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "QueueTokens",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "QueueTokens",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "PatientCheckIns",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "PatientCheckIns",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");
        }
    }
}
