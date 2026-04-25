using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hms.AuthApi.Migrations
{
    public partial class AddRoleBasedStaffLogin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "LoginId", table: "Users", type: "nvarchar(100)", maxLength: 100, nullable: true);
            migrationBuilder.AddColumn<string>(name: "PasswordHash", table: "Users", type: "nvarchar(500)", maxLength: 500, nullable: true);
            migrationBuilder.CreateIndex(name: "IX_Users_LoginId", table: "Users", column: "LoginId", unique: true, filter: "[LoginId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Users_LoginId", table: "Users");
            migrationBuilder.DropColumn(name: "LoginId", table: "Users");
            migrationBuilder.DropColumn(name: "PasswordHash", table: "Users");
        }
    }
}
