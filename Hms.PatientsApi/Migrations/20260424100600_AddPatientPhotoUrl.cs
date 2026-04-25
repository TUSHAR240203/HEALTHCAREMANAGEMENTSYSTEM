using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hms.PatientsApi.Migrations
{
    public partial class AddPatientPhotoUrl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "PhotoUrl", table: "Patients", type: "nvarchar(500)", maxLength: 500, nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "PhotoUrl", table: "Patients");
        }
    }
}
