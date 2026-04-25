using Frontend.Models.Reception;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Frontend.Infrastructure;

namespace Frontend.Controllers
{
    [RequireRole("Admin", "Receptionist")]
    public class ReceptionController : Controller
    {
        private readonly IReceptionApiService _receptionApiService;

        public ReceptionController(IReceptionApiService receptionApiService)
        {
            _receptionApiService = receptionApiService;
        }

        [HttpGet]
        public IActionResult SearchPatients()
        {
            return View(new ReceptionPatientSearchRequestDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SearchPatients(ReceptionPatientSearchRequestDto request)
        {
            try
            {
                var result = await _receptionApiService.SearchPatientsAsync(request);
                ViewBag.Results = result?.Patients ?? new List<ReceptionPatientSummaryDto>();
                return View(request);
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(request);
            }
        }

        [HttpGet]
        public IActionResult RegisterPatient()
        {
            return View(new RegisterPatientByReceptionRequestDto
            {
                DateOfBirth = DateOnly.FromDateTime(DateTime.Today)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterPatient(RegisterPatientByReceptionRequestDto request)
        {
            if (!ModelState.IsValid)
                return View(request);

            try
            {
                await _receptionApiService.RegisterPatientAsync<object>(request);
                TempData["Success"] = "Patient registered successfully.";
                return RedirectToAction(nameof(SearchPatients));
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(request);
            }
        }

        [HttpGet]
        public async Task<IActionResult> PatientSummary(int patientId)
        {
            try
            {
                var result = await _receptionApiService.GetPatientSummaryAsync(patientId);
                if (result == null) return NotFound();
                return View(result);
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(SearchPatients));
            }
        }

        [HttpGet]
        public IActionResult BookAppointment()
        {
            return RedirectToAction("Create", "Appointments");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookAppointment(BookAppointmentRequestDto request)
        {
            if (!ModelState.IsValid)
                return View(request);

            try
            {
                await _receptionApiService.BookAppointmentAsync<object>(request);
                TempData["Success"] = "Appointment booked successfully.";
                return RedirectToAction(nameof(SearchPatients));
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(request);
            }
        }

        [HttpGet]
        public IActionResult CheckIn()
        {
            return View(new CheckInRequestDto
            {
                CheckInTimeUtc = DateTime.UtcNow
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(CheckInRequestDto request)
        {
            if (!ModelState.IsValid)
                return View(request);

            try
            {
                await _receptionApiService.CheckInAsync<object>(request);
                TempData["Success"] = "Patient checked in successfully.";
                return RedirectToAction(nameof(SearchPatients));
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(request);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Queue(int departmentId, DateOnly? date)
        {
            try
            {
                var queueDate = date ?? DateOnly.FromDateTime(DateTime.Today);
                var result = await _receptionApiService.GetQueueAsync(departmentId, queueDate);
                return View(result);
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(SearchPatients));
            }
        }
    }
}
