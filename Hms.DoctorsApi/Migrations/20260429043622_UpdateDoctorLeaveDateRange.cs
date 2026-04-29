using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hms.DoctorsApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDoctorLeaveDateRange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.indexes 
                    WHERE name = 'IX_DoctorLeaves_DoctorId_LeaveDate'
                )
                BEGIN
                    DROP INDEX IX_DoctorLeaves_DoctorId_LeaveDate ON DoctorLeaves;
                END
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('DoctorLeaves', 'StartDate') IS NULL
                BEGIN
                    ALTER TABLE DoctorLeaves ADD StartDate date NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('DoctorLeaves', 'EndDate') IS NULL
                BEGIN
                    ALTER TABLE DoctorLeaves ADD EndDate date NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('DoctorLeaves', 'LeaveDate') IS NOT NULL
                BEGIN
                    EXEC('
                        UPDATE DoctorLeaves
                        SET StartDate = LeaveDate,
                            EndDate = LeaveDate
                        WHERE StartDate IS NULL OR EndDate IS NULL
                    ');
                END
            ");

            migrationBuilder.Sql(@"
                UPDATE DoctorLeaves
                SET StartDate = CAST(GETUTCDATE() AS date)
                WHERE StartDate IS NULL;
            ");

            migrationBuilder.Sql(@"
                UPDATE DoctorLeaves
                SET EndDate = StartDate
                WHERE EndDate IS NULL;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE DoctorLeaves ALTER COLUMN StartDate date NOT NULL;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE DoctorLeaves ALTER COLUMN EndDate date NOT NULL;
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('DoctorLeaves', 'LeaveDate') IS NOT NULL
                BEGIN
                    ALTER TABLE DoctorLeaves DROP COLUMN LeaveDate;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes 
                    WHERE name = 'IX_DoctorLeaves_DoctorId_StartDate_EndDate'
                )
                BEGIN
                    CREATE INDEX IX_DoctorLeaves_DoctorId_StartDate_EndDate
                    ON DoctorLeaves(DoctorId, StartDate, EndDate);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.indexes 
                    WHERE name = 'IX_DoctorLeaves_DoctorId_StartDate_EndDate'
                )
                BEGIN
                    DROP INDEX IX_DoctorLeaves_DoctorId_StartDate_EndDate ON DoctorLeaves;
                END
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('DoctorLeaves', 'LeaveDate') IS NULL
                BEGIN
                    ALTER TABLE DoctorLeaves ADD LeaveDate date NULL;
                END
            ");

            migrationBuilder.Sql(@"
                UPDATE DoctorLeaves
                SET LeaveDate = StartDate
                WHERE LeaveDate IS NULL;
            ");

            migrationBuilder.Sql(@"
                UPDATE DoctorLeaves
                SET LeaveDate = CAST(GETUTCDATE() AS date)
                WHERE LeaveDate IS NULL;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE DoctorLeaves ALTER COLUMN LeaveDate date NOT NULL;
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('DoctorLeaves', 'StartDate') IS NOT NULL
                BEGIN
                    ALTER TABLE DoctorLeaves DROP COLUMN StartDate;
                END
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('DoctorLeaves', 'EndDate') IS NOT NULL
                BEGIN
                    ALTER TABLE DoctorLeaves DROP COLUMN EndDate;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes 
                    WHERE name = 'IX_DoctorLeaves_DoctorId_LeaveDate'
                )
                BEGIN
                    CREATE INDEX IX_DoctorLeaves_DoctorId_LeaveDate
                    ON DoctorLeaves(DoctorId, LeaveDate);
                END
            ");
        }
    }
}