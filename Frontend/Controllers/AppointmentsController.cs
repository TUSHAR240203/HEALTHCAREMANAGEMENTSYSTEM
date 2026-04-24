//using Hms.AppointmentsMvc.Models.Api;
//using Hms.AppointmentsMvc.Models.ViewModels;
//using Hms.AppointmentsMvc.Services;
using Frontend.Services;                // for IAppointmentApiService
using Frontend.Models.ViewModels;      // for AppointmentListViewModel
//using Frontend.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Frontend.Models.Api;
namespace Hms.AppointmentsMvc.Controllers;

public class AppointmentsController : Controller
{
    private readonly IAppointmentApiService _appointmentApiService;

    public AppointmentsController(IAppointmentApiService appointmentApiService)
    {
        _appointmentApiService = appointmentApiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] AppointmentSearchRequestDto search)
    {
        var model = new AppointmentListViewModel { Search = search };

        try
        {
            model.Result = await _appointmentApiService.SearchAsync(search);
            model.Message = TempData["SuccessMessage"]?.ToString();
            model.ErrorMessage = TempData["ErrorMessage"]?.ToString();
        }
        catch (ApiException ex)
        {
            model.ErrorMessage = ex.Message;
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateAppointmentRequestDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAppointmentRequestDto request)
    {
        if (!ModelState.IsValid)
            return View(request);

        try
        {
            var created = await _appointmentApiService.CreateAsync(request);
            TempData["SuccessMessage"] = $"Appointment #{created.Id} created successfully.";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (ApiException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(request);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var appointment = await _appointmentApiService.GetByIdAsync(id);
            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new AppointmentDetailsViewModel
            {
                Appointment = appointment,
                Message = TempData["SuccessMessage"]?.ToString(),
                ErrorMessage = TempData["ErrorMessage"]?.ToString(),
                Reschedule = new RescheduleAppointmentRequestDto
                {
                    NewAppointmentDate = appointment.AppointmentDate,
                    NewSlotStartTime = appointment.SlotStartTime,
                    NewSlotEndTime = appointment.SlotEndTime
                }
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
        try
        {
            var updated = await _appointmentApiService.RescheduleAsync(id, model.Reschedule);

            TempData["SuccessMessage"] = "Appointment rescheduled successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (ApiException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, CancelAppointmentRequestDto request)
    {
        try
        {
            var updated = await _appointmentApiService.CancelAsync(id, request);
            if (updated == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Appointment cancelled successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (ApiException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id, CompleteAppointmentRequestDto request)
    {
        try
        {
            var updated = await _appointmentApiService.CompleteAsync(id, request);
            if (updated == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Appointment marked as completed.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (ApiException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [HttpGet]
    public async Task<IActionResult> ByPatient(int patientId)
    {
        try
        {
            var appointments = await _appointmentApiService.GetByPatientIdAsync(patientId);
            return View(appointments);
        }
        catch (ApiException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
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
}