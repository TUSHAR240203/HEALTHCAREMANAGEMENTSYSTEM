using Frontend.Infrastructure;
using Frontend.Models.Api;
using Frontend.Models.Doctors;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers;

[RequireRole("Admin", "Receptionist", "Doctor", "Patient")]
public class NotificationsController : Controller
{
    private readonly IAppointmentApiService _appointmentApiService;
    private readonly DoctorGatewayService _doctorGatewayService;
    private readonly IReceptionApiService _receptionApiService;

    public NotificationsController(
        IAppointmentApiService appointmentApiService,
        DoctorGatewayService doctorGatewayService,
        IReceptionApiService receptionApiService)
    {
        _appointmentApiService = appointmentApiService;
        _doctorGatewayService = doctorGatewayService;
        _receptionApiService = receptionApiService;
    }

    [HttpGet]
    public async Task<IActionResult> Summary()
    {
        var role = HttpContext.Session.GetString("Role") ?? string.Empty;
        var doctorId = HttpContext.Session.GetInt32("DoctorId") ?? 0;
        var patientId = HttpContext.Session.GetInt32("PatientId") ?? 0;
        var items = new List<NotificationItemDto>();
        var today = DateOnly.FromDateTime(DateTime.Today);
        var nowUtc = DateTime.UtcNow;

        if (Is(role, "Admin") || Is(role, "Receptionist"))
        {
            await AddAdminAndReceptionNotificationsAsync(items, role, today, nowUtc);
        }

        if (Is(role, "Doctor") && doctorId > 0)
        {
            await AddDoctorNotificationsAsync(items, doctorId, today, nowUtc);
        }

        if (Is(role, "Patient") && patientId > 0)
        {
            await AddPatientNotificationsAsync(items, patientId, today);
        }

        var ordered = items
            .GroupBy(x => x.Id)
            .Select(g => g.First())
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(20)
            .ToList();

        return Json(new NotificationSummaryDto
        {
            Count = ordered.Count,
            Items = ordered
        });
    }

    private async Task AddAdminAndReceptionNotificationsAsync(
        List<NotificationItemDto> items,
        string role,
        DateOnly today,
        DateTime nowUtc)
    {
        try
        {
            var pendingLeaves = await _doctorGatewayService.GetLeavesAsync("Pending");
            foreach (var leave in pendingLeaves.Take(8))
            {
                items.Add(new NotificationItemDto
                {
                    Id = $"leave-pending-{leave.Id}-{leave.Status}",
                    Type = "leave",
                    Icon = "bi-calendar-x",
                    Title = "Doctor leave request",
                    Message = $"Leave request from {leave.StartDate:dd MMM} to {leave.EndDate:dd MMM} is pending review.",
                    Url = Url.Action("Leaves", "Doctors") ?? "/Doctors/Leaves",
                    CreatedAtUtc = GetLeaveNotificationTimeUtc(leave)
                });
            }
        }
        catch
        {
            // Notifications are best-effort and must never block the main UI.
        }

        try
        {
            var search = await _appointmentApiService.SearchAsync(new AppointmentSearchRequestDto
            {
                AppointmentDate = today,
                PageNumber = 1,
                PageSize = 10
            });

            foreach (var appointment in search.Appointments.Take(10))
            {
                var status = appointment.DisplayStatus;
                items.Add(new NotificationItemDto
                {
                    Id = $"appointment-{appointment.Id}-{status}-{appointment.AppointmentDate:yyyyMMdd}",
                    Type = "appointment",
                    Icon = GetAppointmentIcon(status),
                    Title = "Appointment update",
                    Message = $"{appointment.DoctorName ?? "Doctor"} • {appointment.DepartmentName ?? "Department"} • {status}",
                    Url = Url.Action("Details", "Appointments", new { id = appointment.Id }) ?? "/Appointments",
                    CreatedAtUtc = GetAppointmentNotificationTimeUtc(appointment)
                });
            }
        }
        catch
        {
            // Ignore API errors in notification polling.
        }

        if (Is(role, "Receptionist") || Is(role, "Admin"))
        {
            try
            {
                var queue = await _receptionApiService.GetQueueAsync(1, today);
                foreach (var token in (queue?.Queue ?? new List<Frontend.Models.Reception.QueueItemDto>()).Take(6))
                {
                    items.Add(new NotificationItemDto
                    {
                        Id = $"queue-{token.QueueTokenId}-{token.Status}",
                        Type = "queue",
                        Icon = "bi-list-ol",
                        Title = "Queue update",
                        Message = $"Token #{token.TokenNumber} • {token.PatientName ?? token.UHID ?? "Patient"} • {token.Status ?? "Waiting"}",
                        Url = Url.Action("Queue", "Reception", new { departmentId = 1 }) ?? "/Reception/Queue?departmentId=1",
                        CreatedAtUtc = today.ToDateTime(TimeOnly.FromTimeSpan(DateTime.Now.TimeOfDay)).ToUniversalTime()
                    });
                }
            }
            catch
            {
                // Ignore API errors in notification polling.
            }
        }
    }

    private async Task AddDoctorNotificationsAsync(List<NotificationItemDto> items, int doctorId, DateOnly today, DateTime nowUtc)
    {
        try
        {
            var appointments = await _appointmentApiService.GetByDoctorIdAsync(doctorId);
            foreach (var appointment in appointments
                .Where(a => a.AppointmentDate >= today.AddDays(-7))
                .OrderByDescending(a => GetAppointmentNotificationTimeUtc(a))
                .Take(10))
            {
                var status = appointment.DisplayStatus;
                items.Add(new NotificationItemDto
                {
                    Id = $"doctor-appointment-{appointment.Id}-{status}",
                    Type = "appointment",
                    Icon = GetAppointmentIcon(status),
                    Title = "Your appointment changed",
                    Message = $"{appointment.AppointmentDate:dd MMM} {appointment.SlotStartTime:hh\\:mm} • {appointment.DepartmentName ?? "Department"} • {status}",
                    Url = Url.Action("Details", "Appointments", new { id = appointment.Id }) ?? "/Appointments",
                    CreatedAtUtc = GetAppointmentNotificationTimeUtc(appointment)
                });
            }
        }
        catch
        {
        }

        try
        {
            var leaves = await _doctorGatewayService.GetLeavesByDoctorAsync(doctorId);
            foreach (var leave in leaves
                .Where(l => ShouldShowLeaveNotification(l, today, nowUtc))
                .OrderByDescending(GetLeaveNotificationTimeUtc)
                .Take(8))
            {
                items.Add(new NotificationItemDto
                {
                    Id = $"doctor-leave-{leave.Id}-{leave.Status}",
                    Type = "leave",
                    Icon = "bi-calendar-check",
                    Title = "Leave status update",
                    Message = $"Your leave from {leave.StartDate:dd MMM} to {leave.EndDate:dd MMM} is {leave.Status}.",
                    Url = Url.Action("Leaves", "Doctors") ?? "/Doctors/Leaves",
                    CreatedAtUtc = GetLeaveNotificationTimeUtc(leave)
                });
            }
        }
        catch
        {
        }
    }

    private async Task AddPatientNotificationsAsync(List<NotificationItemDto> items, int patientId, DateOnly today)
    {
        try
        {
            var appointments = await _appointmentApiService.GetByPatientIdAsync(patientId);
            foreach (var appointment in appointments
                .Where(a => a.AppointmentDate >= today.AddDays(-7))
                .OrderByDescending(a => GetAppointmentNotificationTimeUtc(a))
                .Take(10))
            {
                var status = appointment.DisplayStatus;
                items.Add(new NotificationItemDto
                {
                    Id = $"patient-appointment-{appointment.Id}-{status}",
                    Type = "appointment",
                    Icon = GetAppointmentIcon(status),
                    Title = "Appointment update",
                    Message = $"{appointment.AppointmentDate:dd MMM} {appointment.SlotStartTime:hh\\:mm} with {appointment.DoctorName ?? "your doctor"} • {status}",
                    Url = Url.Action("Details", "Appointments", new { id = appointment.Id }) ?? "/Appointments/ByPatient",
                    CreatedAtUtc = GetAppointmentNotificationTimeUtc(appointment)
                });
            }
        }
        catch
        {
        }
    }

    private static bool ShouldShowLeaveNotification(DoctorLeaveResponseDto leave, DateOnly today, DateTime nowUtc)
    {
        if (leave.Status.Contains("pending", StringComparison.OrdinalIgnoreCase))
        {
            return leave.EndDate >= today.AddDays(-1);
        }

        if (leave.ReviewedAtUtc.HasValue)
        {
            return leave.ReviewedAtUtc.Value >= nowUtc.AddDays(-14);
        }

        return false;
    }

    private static DateTime GetLeaveNotificationTimeUtc(DoctorLeaveResponseDto leave)
    {
        if (leave.ReviewedAtUtc.HasValue) return DateTime.SpecifyKind(leave.ReviewedAtUtc.Value, DateTimeKind.Utc);
        return DateTime.SpecifyKind(leave.StartDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Local).ToUniversalTime();
    }

    private static DateTime GetAppointmentNotificationTimeUtc(AppointmentResponseDto appointment)
    {
        if (appointment.CreatedAtUtc != default) return DateTime.SpecifyKind(appointment.CreatedAtUtc, DateTimeKind.Utc);
        return DateTime.SpecifyKind(appointment.AppointmentDate.ToDateTime(appointment.SlotStartTime), DateTimeKind.Local).ToUniversalTime();
    }

    private static string GetAppointmentIcon(string status)
    {
        if (status.Contains("cancel", StringComparison.OrdinalIgnoreCase)) return "bi-calendar-x";
        if (status.Contains("complete", StringComparison.OrdinalIgnoreCase)) return "bi-check-circle";
        if (status.Contains("reschedule", StringComparison.OrdinalIgnoreCase)) return "bi-calendar2-event";
        if (status.Contains("check", StringComparison.OrdinalIgnoreCase)) return "bi-person-check";
        return "bi-calendar2-week";
    }

    private static bool Is(string actual, string expected)
        => string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
}

public class NotificationSummaryDto
{
    public int Count { get; set; }
    public List<NotificationItemDto> Items { get; set; } = new();
}

public class NotificationItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = "info";
    public string Icon { get; set; } = "bi-bell";
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Url { get; set; } = "/";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
