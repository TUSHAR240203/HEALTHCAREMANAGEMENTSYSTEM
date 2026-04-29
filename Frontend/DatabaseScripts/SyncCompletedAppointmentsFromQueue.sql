-- Run this once in SSMS against your HMS database.
-- It updates existing Appointments whose linked QueueTokens are already Completed.
-- Status values used by the app:
--   Appointments.Status: 1=Booked, 2=Rescheduled, 3=Cancelled, 4=Completed, 5=CheckedIn
-- If QueueTokens.Status is INT, the TRY_CONVERT condition works. If it is NVARCHAR, the text condition works.

UPDATE a
SET
    a.Status = 4,
    a.CompletionNotes = COALESCE(a.CompletionNotes, qt.CompletionNotes, qt.Notes, 'Completed from queue'),
    a.UpdatedAtUtc = SYSUTCDATETIME()
FROM dbo.Appointments a
INNER JOIN dbo.QueueTokens qt ON qt.AppointmentId = a.Id
WHERE
    a.Status <> 4
    AND (
        qt.Status = 'Completed'
        OR TRY_CONVERT(int, qt.Status) = 4
    );
