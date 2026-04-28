using Hms.ReceptionApi.Data;
using Hms.ReceptionApi.Entities;

namespace Hms.ReceptionApi.Tests.TestHelpers;

public static class TestSeeder
{
    public static async Task SeedAsync(ReceptionDbContext context)
    {
        if (!context.QueueTokens.Any())
        {
            context.QueueTokens.Add(new QueueToken
            {
                DepartmentId = 1,
                QueueDate = DateOnly.FromDateTime(DateTime.Today),
                TokenNumber = 101,
                PatientId = 1,
                UHID = "UHID001",
                PatientName = "Tushar Sharma",
                AppointmentId = 1,
                DoctorId = 1,
                Status = "Waiting"
            });
        }

        if (!context.PatientCheckIns.Any())
        {
            context.PatientCheckIns.Add(new PatientCheckIn
            {
                PatientId = 1,
                UHID = "UHID001",
                AppointmentId = 1,
                DoctorId = 1,
                DepartmentId = 1,
                CheckInTimeUtc = DateTime.UtcNow,
                TokenNumber = 101,
                Status = "CheckedIn"
            });
        }

        await context.SaveChangesAsync();
    }
}