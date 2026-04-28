using Hms.ReceptionApi.DTOs.Reception;
using Hms.ReceptionApi.Entities;

namespace Hms.ReceptionApi.Tests.TestHelpers;

public static class MockData
{
    public static ReceptionPatientSummaryDto GetPatient()
    {
        return new ReceptionPatientSummaryDto
        {
            PatientId = 1,
            UHID = "UHID001",
            FullName = "Tushar Sharma",
            DateOfBirth = new DateOnly(2003, 2, 24),
            Gender = 1,
            MobileNumber = "9999999999",
            Email = "tushar@gmail.com",
            PortalAccessEnabled = true,
            PortalActivated = false,
            Status = 1
        };
    }

    public static QueueToken GetQueueToken()
    {
        return new QueueToken
        {
            Id = 1,
            DepartmentId = 1,
            QueueDate = DateOnly.FromDateTime(DateTime.Today),
            TokenNumber = 101,
            PatientId = 1,
            UHID = "UHID001",
            PatientName = "Tushar Sharma",
            AppointmentId = 1,
            DoctorId = 1,
            Status = "Waiting"
        };
    }

    public static PatientCheckIn GetCheckIn()
    {
        return new PatientCheckIn
        {
            Id = 1,
            PatientId = 1,
            UHID = "UHID001",
            AppointmentId = 1,
            DoctorId = 1,
            DepartmentId = 1,
            CheckInTimeUtc = DateTime.UtcNow,
            TokenNumber = 101,
            Status = "CheckedIn"
        };
    }

    public static InvoiceResponseDto GetInvoice()
    {
        return new InvoiceResponseDto
        {
            Id = 1,
            PatientId = 1,
            UHID = "UHID001",
            AppointmentId = 1,
            TotalAmount = 500,
            PaidAmount = 0,
            BalanceAmount = 500,
            Status = "Pending",
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}