using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hms.ReceptionApi.Migrations
{
    /// <inheritdoc />
    public partial class recptionistinit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PatientCheckIns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    UHID = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AppointmentId = table.Column<int>(type: "int", nullable: false),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    CheckInTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TokenNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientCheckIns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QueueTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    QueueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TokenNumber = table.Column<int>(type: "int", nullable: false),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    UHID = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PatientName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AppointmentId = table.Column<int>(type: "int", nullable: false),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CalledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SkippedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueueTokens", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PatientCheckIns_AppointmentId",
                table: "PatientCheckIns",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientCheckIns_DepartmentId",
                table: "PatientCheckIns",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientCheckIns_DepartmentId_CheckInTimeUtc",
                table: "PatientCheckIns",
                columns: new[] { "DepartmentId", "CheckInTimeUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientCheckIns_DoctorId",
                table: "PatientCheckIns",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientCheckIns_PatientId",
                table: "PatientCheckIns",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientCheckIns_PatientId_AppointmentId",
                table: "PatientCheckIns",
                columns: new[] { "PatientId", "AppointmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QueueTokens_AppointmentId",
                table: "QueueTokens",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_QueueTokens_DepartmentId_QueueDate_Status",
                table: "QueueTokens",
                columns: new[] { "DepartmentId", "QueueDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_QueueTokens_DepartmentId_QueueDate_TokenNumber",
                table: "QueueTokens",
                columns: new[] { "DepartmentId", "QueueDate", "TokenNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QueueTokens_DoctorId",
                table: "QueueTokens",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_QueueTokens_PatientId",
                table: "QueueTokens",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientCheckIns");

            migrationBuilder.DropTable(
                name: "QueueTokens");
        }
    }
}
