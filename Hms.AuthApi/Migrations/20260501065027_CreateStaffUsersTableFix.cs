using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hms.AuthApi.Migrations
{
    /// <inheritdoc />
    public partial class CreateStaffUsersTableFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[dbo].[StaffUsers]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[StaffUsers] (
                        [Id] int NOT NULL IDENTITY(1,1),
                        [UserId] int NOT NULL,
                        [FullName] nvarchar(150) NOT NULL,
                        [CreatedAtUtc] datetime2 NOT NULL,
                        [UpdatedAtUtc] datetime2 NULL,
                        [IsDeleted] bit NOT NULL,
                        CONSTRAINT [PK_StaffUsers] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_StaffUsers_Users_UserId]
                            FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE
                    );

                    CREATE UNIQUE INDEX [IX_StaffUsers_UserId]
                    ON [dbo].[StaffUsers] ([UserId]);
                END
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.Users', 'FullName') IS NOT NULL
                BEGIN
                    EXEC(N'
                        INSERT INTO [dbo].[StaffUsers]
                            ([UserId], [FullName], [CreatedAtUtc], [UpdatedAtUtc], [IsDeleted])
                        SELECT
                            u.[Id],
                            ISNULL(NULLIF(u.[FullName], ''''), u.[LoginId]),
                            u.[CreatedAtUtc],
                            u.[UpdatedAtUtc],
                            u.[IsDeleted]
                        FROM [dbo].[Users] u
                        INNER JOIN [dbo].[UserRoles] ur ON ur.[UserId] = u.[Id]
                        INNER JOIN [dbo].[Roles] r ON r.[Id] = ur.[RoleId]
                        WHERE r.[NormalizedName] IN (''ADMIN'', ''RECEPTIONIST'')
                          AND NOT EXISTS (
                              SELECT 1
                              FROM [dbo].[StaffUsers] s
                              WHERE s.[UserId] = u.[Id]
                          );
                    ');

                    ALTER TABLE [dbo].[Users] DROP COLUMN [FullName];
                END
                ELSE
                BEGIN
                    INSERT INTO [dbo].[StaffUsers]
                        ([UserId], [FullName], [CreatedAtUtc], [UpdatedAtUtc], [IsDeleted])
                    SELECT
                        u.[Id],
                        CASE
                            WHEN r.[NormalizedName] = 'ADMIN' THEN 'Admin'
                            WHEN r.[NormalizedName] = 'RECEPTIONIST' THEN 'Reception'
                            ELSE ISNULL(u.[LoginId], 'Staff')
                        END,
                        u.[CreatedAtUtc],
                        u.[UpdatedAtUtc],
                        u.[IsDeleted]
                    FROM [dbo].[Users] u
                    INNER JOIN [dbo].[UserRoles] ur ON ur.[UserId] = u.[Id]
                    INNER JOIN [dbo].[Roles] r ON r.[Id] = ur.[RoleId]
                    WHERE r.[NormalizedName] IN ('ADMIN', 'RECEPTIONIST')
                      AND NOT EXISTS (
                          SELECT 1
                          FROM [dbo].[StaffUsers] s
                          WHERE s.[UserId] = u.[Id]
                      );
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.Users', 'FullName') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[Users]
                    ADD [FullName] nvarchar(150) NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[dbo].[StaffUsers]', N'U') IS NOT NULL
                BEGIN
                    EXEC(N'
                        UPDATE u
                        SET u.[FullName] = s.[FullName]
                        FROM [dbo].[Users] u
                        INNER JOIN [dbo].[StaffUsers] s ON s.[UserId] = u.[Id];
                    ');

                    DROP TABLE [dbo].[StaffUsers];
                END
            ");
        }
    }
}
