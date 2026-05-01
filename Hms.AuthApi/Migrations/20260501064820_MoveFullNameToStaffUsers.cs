using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hms.AuthApi.Migrations
{
    /// <inheritdoc />
    public partial class MoveFullNameToStaffUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Kept as an empty migration because this migration was already applied in development.
            // The real StaffUsers table/data migration is 20260501065027_CreateStaffUsersTableFix.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
