-- Optional permanent SQL safety net.
-- Create this trigger if you want SQL Server to always keep Appointments.Status in sync
-- whenever QueueTokens.Status becomes Completed.
-- Run this in SSMS against your HMS database.

CREATE OR ALTER TRIGGER dbo.trg_QueueTokens_CompleteAppointment
ON dbo.QueueTokens
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE a
    SET
        a.Status = 4,
        a.CompletionNotes = COALESCE(a.CompletionNotes, i.CompletionNotes, i.Notes, 'Completed from queue'),
        a.UpdatedAtUtc = SYSUTCDATETIME()
    FROM dbo.Appointments a
    INNER JOIN inserted i ON i.AppointmentId = a.Id
    WHERE
        i.AppointmentId IS NOT NULL
        AND i.AppointmentId > 0
        AND a.Status <> 4
        AND (
            i.Status = 'Completed'
            OR TRY_CONVERT(int, i.Status) = 4
        );
END;
GO
