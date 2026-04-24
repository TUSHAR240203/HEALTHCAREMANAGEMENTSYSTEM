using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hms.PatientsApi.Migrations
{
    /// <inheritdoc />
    public partial class AllowSharedMobileNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Country",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Patients");

            migrationBuilder.AddColumn<string>(
                name: "PatientIdentifier",
                table: "Patients",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MobileNumberChangeRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    ExistingOwnerPatientId = table.Column<int>(type: "int", nullable: false),
                    NewMobileNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OtpCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsConsumed = table.Column<bool>(type: "bit", nullable: false),
                    VerifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileNumberChangeRequests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Patients_PatientIdentifier",
                table: "Patients",
                column: "PatientIdentifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MobileNumberChangeRequests_PatientId_NewMobileNumber_IsConsumed",
                table: "MobileNumberChangeRequests",
                columns: new[] { "PatientId", "NewMobileNumber", "IsConsumed" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MobileNumberChangeRequests");

            migrationBuilder.DropIndex(
                name: "IX_Patients_PatientIdentifier",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "PatientIdentifier",
                table: "Patients");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Patients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Patients",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");
        }
    }
}
