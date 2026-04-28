using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hms.AuthApi.Migrations
{
    public partial class AddAuthPreferenceFlags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(name: "IsPasswordLoginEnabled", table: "Users", type: "bit", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<bool>(name: "IsOtpLoginEnabled", table: "Users", type: "bit", nullable: false, defaultValue: true);
            migrationBuilder.AddColumn<bool>(name: "IsFirstLoginCompleted", table: "Users", type: "bit", nullable: false, defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "IsPasswordLoginEnabled", table: "Users");
            migrationBuilder.DropColumn(name: "IsOtpLoginEnabled", table: "Users");
            migrationBuilder.DropColumn(name: "IsFirstLoginCompleted", table: "Users");
        }
    }
}
