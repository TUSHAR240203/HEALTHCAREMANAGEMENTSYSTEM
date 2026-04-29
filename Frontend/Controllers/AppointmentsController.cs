using Frontend.Infrastructure;
using Frontend.Models.Api;
using Frontend.Models.ViewModels;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers;

[RequireRole("Admin", "Receptionist", "Doctor", "Patient")]
public class AppointmentsController : Controller
{
    private readonly IAppointmentApiService _appointmentApiService;
    private readonly DoctorGatewayService _doctorGatewayService;
    private readonly PatientGatewayService _patientGatewayService;

    public AppointmentsController(
        IAppointmentApiService appointmentApiService,
        DoctorGatewayService doctorGatewayService,
        PatientGatewayService patientGatewayService)
    {
        _appointmentApiService = appointmentApiService;
        _doctorGatewayService = doctorGatewayService;
        _patientGatewayService = patientGatewayService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] AppointmentSearchRequestDto search)
    {
        if (UserIsPatient())
        {
            var patientId = HttpContext.Session.GetInt32("PatientId") ?? 0;
            if (patientId > 0) return RedirectToAction(nameof(ByPatient), new { patientId });
        }

        var model = new AppointmentListViewModel { Search = search };
        try
        {
            model.Result = await _appointmentApiService.SearchAsync(search);
            model.Message = TempData["SuccessMessage"]?.ToString() ?? TempData["Success"]?.ToString();
            model.ErrorMessage = TempData["ErrorMessage"]?.ToString() ?? TempData["Error"]?.ToString();
        }
        catch (ApiException ex)
        {
            model.ErrorMessage = ex.Message;
        }
        return View(model);
    }

    [HttpGet]
    [RequireRole("Admin", "Receptionist")]
    public async Task<IActionResult> Create(int? doctorId, DateOnly? appointmentDate)
    {
        return View(await BuildAdminBookingModelAsync(doctorId, appointmentDate, null));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequireRole("Admin", "Receptionist")]
    public async Task<IActionResult> Create(AdminAppointmentBookingViewModel model)
    {
        if (!TryParseSlot(model.SelectedSlot, out var start, out var end))
        {
            ModelState.AddModelError(nameof(model.SelectedSlot), "Please select a valid time slot.");
        }

        var hydrated = await BuildAdminBookingModelAsync(
            model.DoctorId,
            model.AppointmentDate,
            model.SelectedSlot);

        model.Doctors = hydrated.Doctors;
        model.Slots = hydrated.Slots;

        var doctor = model.Doctors.FirstOrDefault(d => d.Id == model.DoctorId);

        if (doctor == null)
        {
            ModelState.AddModelError(nameof(model.DoctorId), "Please select an active doctor.");
        }

        var chosenSlot = model.Slots.FirstOrDefault(s => s.Value == model.SelectedSlot);

        if (chosenSlot?.IsBooked == true)
        {
            ModelState.AddModelError(nameof(model.SelectedSlot), "This slot is already booked. Please choose another slot.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var created = await _appointmentApiService.CreateAsync(new CreateAppointmentRequestDto
            {
                PatientId = model.PatientId,
                UHID = model.UHID,
                DoctorId = doctor!.Id,
                DoctorName = doctor.FullName,
                DepartmentId = doctor.DepartmentId,
                DepartmentName = doctor.DepartmentName,

                // IMPORTANT: this uses the selected calendar date
                AppointmentDate = model.AppointmentDate,

                SlotStartTime = start,
                SlotEndTime = end,
                VisitType = model.VisitType,
                ReasonForVisit = model.ReasonForVisit,
                IsTeleConsultation = model.IsTeleConsultation
            });

            TempData["SuccessMessage"] = $"Appointment #{created.Id} created successfully.";

            return RedirectToAction(nameof(Details), new
            {
                id = created.Id
            });
        }
        catch (ApiException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);

            var reload = await BuildAdminBookingModelAsync(
                model.DoctorId,
                model.AppointmentDate,
                model.SelectedSlot);

            model.Doctors = reload.Doctors;
            model.Slots = reload.Slots;

            return View(model);
        }
    }

    [HttpGet]
    [RequireRole("Patient")]
    public async Task<IActionResult> PatientBook(int? doctorId, DateOnly? appointmentDate)
    {
        var model = await BuildPatientBookingModelAsync(doctorId, appointmentDate, null);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequireRole("Patient")]
    public async Task<IActionResult> PatientBook(PatientAppointmentBookingViewModel model)
    {
        model.PatientId = HttpContext.Session.GetInt32("PatientId") ?? 0;
        model.UHID = HttpContext.Session.GetString("UHID") ?? string.Empty;

        if (model.PatientId <= 0)
        {
            TempData["Error"] = "Please login again as a patient before booking an appointment.";
            return RedirectToAction("Login", "Account");
        }

        if (string.IsNullOrWhiteSpace(model.UHID))
        {
            var patient = await _patientGatewayService.GetByIdAsync(model.PatientId);
            model.UHID = patient?.UHID ?? string.Empty;
        }

        if (!TryParseSlot(model.SelectedSlot, out var start, out var end))
            ModelState.AddModelError(nameof(model.SelectedSlot), "Please select a valid time slot.");

        var hydrated = await BuildPatientBookingModelAsync(model.DoctorId, model.AppointmentDate, model.SelectedSlot);
        model.Doctors = hydrated.Doctors;
        model.Slots = hydrated.Slots;

        var doctor = model.Doctors.FirstOrDefault(d => d.Id == model.DoctorId);
        if (doctor == null) ModelState.AddModelError(nameof(model.DoctorId), "Please select a valid doctor.");
        if (string.IsNullOrWhiteSpace(model.UHID)) ModelState.AddModelError(string.Empty, "UHID is missing. Please contact reception.");

        var chosenSlot = model.Slots.FirstOrDefault(s => s.Value == model.SelectedSlot);
        if (chosenSlot?.IsBooked == true) ModelState.AddModelError(nameof(model.SelectedSlot), "This slot was just booked. Please choose another slot.");

        if (!ModelState.IsValid) return View(model);

        try
        {
            var request = new CreateAppointmentRequestDto
            {
                PatientId = model.PatientId,
                UHID = model.UHID,
                DoctorId = doctor!.Id,
                DoctorName = doctor.FullName,
                DepartmentId = doctor.DepartmentId,
                DepartmentName = doctor.DepartmentName,
                AppointmentDate = model.AppointmentDate,
                SlotStartTime = start,
                SlotEndTime = end,
                VisitType = model.VisitType,
                ReasonForVisit = model.ReasonForVisit,
                IsTeleConsultation = model.IsTeleConsultation
            };

            var created = await _appointmentApiService.CreateAsync(request);
            TempData["Success"] = $"Appointment #{created.Id} booked successfully.";
            return RedirectToAction(nameof(ByPatient), new { patientId = model.PatientId });
        }
        catch (ApiException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, DateOnly? rescheduleDate, int? rescheduleDoctorId)
    {
        try
        {
            var appointment = await _appointmentApiService.GetByIdAsync(id);
            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction(nameof(Index));
            }

            var chosenRescheduleDate = rescheduleDate ?? DateOnly.FromDateTime(DateTime.Today);
            var doctors = await _doctorGatewayService.GetAllAsync(true);
            var selectedRescheduleDoctorId = rescheduleDoctorId.GetValueOrDefault(appointment.DoctorId);
            var doctor = await _doctorGatewayService.GetByIdAsync(appointment.DoctorId);
            var patient = await _patientGatewayService.GetByIdAsync(appointment.PatientId);
            var model = new AppointmentDetailsViewModel
            {
                Appointment = appointment,
                Doctor = doctor,
                Doctors = doctors,
                RescheduleDoctorId = selectedRescheduleDoctorId,
                Patient = patient,
                Message = TempData["SuccessMessage"]?.ToString(),
                ErrorMessage = TempData["ErrorMessage"]?.ToString(),
                Reschedule = new RescheduleAppointmentRequestDto
                {
                    NewAppointmentDate = chosenRescheduleDate,
                    NewSlotStartTime = appointment.SlotStartTime,
                    NewSlotEndTime = appointment.SlotEndTime
                },
                FreeSlots = await BuildSlotsAsync(selectedRescheduleDoctorId, chosenRescheduleDate, selectedRescheduleDoctorId == appointment.DoctorId ? appointment.Id : null)
            };

            return View(model);
        }
        catch (ApiException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reschedule(int id, AppointmentDetailsViewModel model)
    {
        if (!TryParseSlot(model.SelectedSlot, out var start, out var end))
        {
            TempData["ErrorMessage"] = "Please select one free slot before rescheduling.";
            return RedirectToAction(nameof(Details), new { id, rescheduleDate = model.Reschedule.NewAppointmentDate });
        }

        model.Reschedule.NewSlotStartTime = start;
        model.Reschedule.NewSlotEndTime = end;

        try
        {
            await _appointmentApiService.RescheduleAsync(id, model.Reschedule);
            TempData["SuccessMessage"] = "Appointment rescheduled successfully.";
        }
        catch (ApiException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, CancelAppointmentRequestDto request)
    {
        try
        {
            await _appointmentApiService.CancelAsync(id, request);
            TempData["SuccessMessage"] = "Appointment cancelled successfully.";
        }
        catch (ApiException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequireRole("Admin", "Doctor")]
    public async Task<IActionResult> Complete(int id, CompleteAppointmentRequestDto request)
    {
        try
        {
            await _appointmentApiService.CompleteAsync(id, request);
            TempData["SuccessMessage"] = "Appointment marked as completed.";
        }
        catch (ApiException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> ByPatient(int patientId)
    {
        if (UserIsPatient())
        {
            var sessionPatientId = HttpContext.Session.GetInt32("PatientId") ?? 0;
            if (sessionPatientId > 0) patientId = sessionPatientId;
        }

        try
        {
            var appointments = await _appointmentApiService.GetByPatientIdAsync(patientId);
            ViewBag.PatientId = patientId;
            return View(appointments);
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
            return View(new List<AppointmentResponseDto>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> ByDoctor(int doctorId)
    {
        try
        {
            var appointments = await _appointmentApiService.GetByDoctorIdAsync(doctorId);
            return View(appointments);
        }
        catch (ApiException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task<AdminAppointmentBookingViewModel> BuildAdminBookingModelAsync(
     int? doctorId,
     DateOnly? appointmentDate,
     string? selectedSlot)
    {
        var doctors = await _doctorGatewayService.GetAllAsync(true);

        // IMPORTANT: only default to today when no date was selected
        var date = appointmentDate ?? DateOnly.FromDateTime(DateTime.Today);

        var selectedDoctorId = doctorId.GetValueOrDefault();

        if (selectedDoctorId <= 0 && doctors.Count > 0)
        {
            selectedDoctorId = doctors[0].Id;
        }

        return new AdminAppointmentBookingViewModel
        {
            DoctorId = selectedDoctorId,
            AppointmentDate = date,
            SelectedSlot = selectedSlot ?? string.Empty,
            Doctors = doctors,
            Slots = selectedDoctorId > 0
                ? await BuildSlotsAsync(selectedDoctorId, date)
                : new List<PatientSlotOption>()
        };
    }

    private async Task<PatientAppointmentBookingViewModel> BuildPatientBookingModelAsync(int? doctorId, DateOnly? appointmentDate, string? selectedSlot)
    {
        var patientId = HttpContext.Session.GetInt32("PatientId") ?? 0;
        var uhid = HttpContext.Session.GetString("UHID") ?? string.Empty;
        var doctors = (await _doctorGatewayService.GetAllAsync(true));
        var date = appointmentDate ?? DateOnly.FromDateTime(DateTime.Today);
        var selectedDoctorId = doctorId.GetValueOrDefault();
        if (selectedDoctorId <= 0 && doctors.Count > 0) selectedDoctorId = doctors[0].Id;

        if (patientId > 0 && string.IsNullOrWhiteSpace(uhid))
        {
            var patient = await _patientGatewayService.GetByIdAsync(patientId);
            uhid = patient?.UHID ?? string.Empty;
        }

        return new PatientAppointmentBookingViewModel
        {
            PatientId = patientId,
            UHID = uhid,
            DoctorId = selectedDoctorId,
            AppointmentDate = date,
            SelectedSlot = selectedSlot ?? string.Empty,
            Doctors = doctors,
            Slots = selectedDoctorId > 0 ? await BuildSlotsAsync(selectedDoctorId, date) : new List<PatientSlotOption>()
        };
    }

    private async Task<List<PatientSlotOption>> BuildSlotsAsync(int doctorId, DateOnly appointmentDate, int? ignoreAppointmentId = null)
    {
        var result = await _appointmentApiService.SearchAsync(new AppointmentSearchRequestDto
        {
            DoctorId = doctorId,
            AppointmentDate = appointmentDate,
            PageNumber = 1,
            PageSize = 100
        });

        var booked = result.Appointments
            .Where(a => a.Status != AppointmentStatus.Cancelled && (!ignoreAppointmentId.HasValue || a.Id != ignoreAppointmentId.Value))
            .Select(a => $"{a.SlotStartTime:HH\\:mm}|{a.SlotEndTime:HH\\:mm}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var slots = new List<PatientSlotOption>();
        var start = new TimeOnly(9, 0);
        var endOfDay = new TimeOnly(17, 0);
        while (start < endOfDay)
        {
            var end = start.AddMinutes(30);
            var value = $"{start:HH\\:mm}|{end:HH\\:mm}";
            slots.Add(new PatientSlotOption
            {
                Start = start,
                End = end,
                Value = value,
                Label = $"{start:hh\\:mm tt} - {end:hh\\:mm tt}",
                IsBooked = booked.Contains(value)
            });
            start = end;
        }
        return slots;
    }

    private static bool TryParseSlot(string? selectedSlot, out TimeOnly start, out TimeOnly end)
    {
        start = default;
        end = default;
        if (string.IsNullOrWhiteSpace(selectedSlot)) return false;
        var parts = selectedSlot.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length == 2 && TimeOnly.TryParse(parts[0], out start) && TimeOnly.TryParse(parts[1], out end);
    }

    private bool UserIsPatient() => string.Equals(HttpContext.Session.GetString("Role"), "Patient", StringComparison.OrdinalIgnoreCase);
}
