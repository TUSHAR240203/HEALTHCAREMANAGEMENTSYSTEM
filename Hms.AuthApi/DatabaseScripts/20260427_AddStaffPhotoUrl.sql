IF COL_LENGTH('Users', 'PhotoUrl') IS NULL
BEGIN
    ALTER TABLE [Users] ADD [PhotoUrl] nvarchar(500) NULL;
END
GO
