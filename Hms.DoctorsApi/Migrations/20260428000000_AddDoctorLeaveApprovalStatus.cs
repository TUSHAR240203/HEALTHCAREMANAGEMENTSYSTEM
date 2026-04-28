using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hms.DoctorsApi.Migrations
{
    public partial class AddDoctorLeaveApprovalStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Status", table: "DoctorLeaves");
            migrationBuilder.DropColumn(name: "ReviewedAtUtc", table: "DoctorLeaves");
            migrationBuilder.DropColumn(name: "ReviewedBy", table: "DoctorLeaves");
        }
    }
}
