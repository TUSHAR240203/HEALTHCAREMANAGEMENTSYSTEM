IF COL_LENGTH('Doctors', 'AuthUserId') IS NULL
BEGIN
    ALTER TABLE Doctors ADD AuthUserId int NULL;
END
GO

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
GO
