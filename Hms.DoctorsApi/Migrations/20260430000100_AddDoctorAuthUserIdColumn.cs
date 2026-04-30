using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hms.DoctorsApi.Migrations
{
    [Migration("20260430000100_AddDoctorAuthUserIdColumn")]
    public partial class AddDoctorAuthUserIdColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Doctors', 'AuthUserId') IS NULL
                BEGIN
                    ALTER TABLE Doctors ADD AuthUserId int NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_Doctors_AuthUserId'
                      AND object_id = OBJECT_ID('Doctors')
                )
                BEGIN
                    CREATE UNIQUE INDEX IX_Doctors_AuthUserId
                    ON Doctors(AuthUserId)
                    WHERE AuthUserId IS NOT NULL;
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_Doctors_AuthUserId'
                      AND object_id = OBJECT_ID('Doctors')
                )
                BEGIN
                    DROP INDEX IX_Doctors_AuthUserId ON Doctors;
                END
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('Doctors', 'AuthUserId') IS NOT NULL
                BEGIN
                    ALTER TABLE Doctors DROP COLUMN AuthUserId;
                END
            ");
        }
    }
}
