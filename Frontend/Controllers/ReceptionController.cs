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

                if (result == null)
                    return NotFound();

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
        public async Task<IActionResult> CheckIn(DateOnly? date = null)
        {
            var checkInDate = date ?? DateOnly.FromDateTime(DateTime.Today);

            var model = new CheckInPageViewModel
            {
                Date = checkInDate,
                CheckIn = new CheckInRequestDto
                {
                    QueueDate = checkInDate,
                    CheckInTimeUtc = DateTime.UtcNow
                },
                Appointments = new List<TodayAppointmentForCheckInDto>()
            };

            try
            {
                model.Appointments =
                    await _receptionApiService.GetTodayScheduledAppointmentsForCheckInAsync(checkInDate);
            }
            catch (ApiException ex)
            {
                model.ErrorMessage = ex.Message;
            }
            catch (HttpRequestException ex)
            {
                model.ErrorMessage =
                    $"Could not load today's appointments. API connection failed: {ex.Message}";
            }
            catch (Exception ex)
            {
                model.ErrorMessage =
                    $"Could not load today's appointments. Details: {ex.Message}";
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(CheckInRequestDto request)
        {
            if (request.QueueDate == default)
                request.QueueDate = DateOnly.FromDateTime(DateTime.Today);

            if (request.CheckInTimeUtc == default)
                request.CheckInTimeUtc = DateTime.UtcNow;

            if (!ModelState.IsValid)
            {
                var invalidModel = await BuildCheckInModelAsync(request);
                return View(invalidModel);
            }

            try
            {
                await _receptionApiService.CheckInAsync<object>(request);

                TempData["Success"] = "Patient checked in successfully and added to the queue.";

                return RedirectToAction(
                    nameof(Queue),
                    new
                    {
                        departmentId = request.DepartmentId,
                        date = request.QueueDate.ToString("yyyy-MM-dd")
                    });
            }
            catch (ApiException ex)
            {
                var errorModel = await BuildCheckInModelAsync(request);
                errorModel.ErrorMessage = ex.Message;
                return View(errorModel);
            }
            catch (HttpRequestException ex)
            {
                var errorModel = await BuildCheckInModelAsync(request);
                errorModel.ErrorMessage =
                    $"Could not complete check-in. API connection failed: {ex.Message}";
                return View(errorModel);
            }
            catch (Exception ex)
            {
                var errorModel = await BuildCheckInModelAsync(request);
                errorModel.ErrorMessage =
                    $"Could not complete check-in. Details: {ex.Message}";
                return View(errorModel);
            }
        }

        private async Task<CheckInPageViewModel> BuildCheckInModelAsync(CheckInRequestDto request)
        {
            var model = new CheckInPageViewModel
            {
                Date = request.QueueDate,
                CheckIn = request,
                Appointments = new List<TodayAppointmentForCheckInDto>()
            };

            try
            {
                model.Appointments =
                    await _receptionApiService.GetTodayScheduledAppointmentsForCheckInAsync(request.QueueDate);
            }
            catch (ApiException ex)
            {
                model.ErrorMessage = ex.Message;
            }
            catch (HttpRequestException ex)
            {
                model.ErrorMessage =
                    $"Could not reload appointments. API connection failed: {ex.Message}";
            }
            catch (Exception ex)
            {
                model.ErrorMessage =
                    $"Could not reload appointments. Details: {ex.Message}";
            }

            return model;
        }

        [HttpGet]
        public async Task<IActionResult> Queue(int departmentId = 1, DateOnly? date = null)
        {
            try
            {
                var queueDate = date ?? DateOnly.FromDateTime(DateTime.Today);

                var result = await _receptionApiService.GetQueueAsync(departmentId, queueDate);

                ViewBag.CurrentQueue =
                    await _receptionApiService.GetCurrentQueueAsync(departmentId, queueDate);

                return View(result);
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(SearchPatients));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(SearchPatients));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CallNext(int departmentId, DateOnly date)
        {
            try
            {
                await _receptionApiService.CallNextAsync<object>(departmentId, date);
                TempData["Success"] = "Next patient called.";
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Queue), new { departmentId, date = date.ToString("yyyy-MM-dd") });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartToken(int queueTokenId, int departmentId, DateOnly date)
        {
            try
            {
                await _receptionApiService.StartTokenAsync<object>(queueTokenId);
                TempData["Success"] = "Consultation started.";
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Queue), new { departmentId, date = date.ToString("yyyy-MM-dd") });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteToken(
            int queueTokenId,
            int departmentId,
            DateOnly date,
            string? notes)
        {
            try
            {
                await _receptionApiService.CompleteTokenAsync<object>(
                    queueTokenId,
                    new CompleteQueueTokenRequestDto { Notes = notes });

                TempData["Success"] = "Patient completed.";
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Queue), new { departmentId, date = date.ToString("yyyy-MM-dd") });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SkipToken(int queueTokenId, int departmentId, DateOnly date)
        {
            try
            {
                await _receptionApiService.SkipTokenAsync<object>(queueTokenId);
                TempData["Success"] = "Patient skipped.";
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Queue), new { departmentId, date = date.ToString("yyyy-MM-dd") });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecallToken(int queueTokenId, int departmentId, DateOnly date)
        {
            try
            {
                await _receptionApiService.RecallTokenAsync<object>(queueTokenId);
                TempData["Success"] = "Patient recalled.";
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Queue), new { departmentId, date = date.ToString("yyyy-MM-dd") });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelToken(
            int queueTokenId,
            int departmentId,
            DateOnly date,
            string? notes)
        {
            try
            {
                await _receptionApiService.CancelTokenAsync<object>(
                    queueTokenId,
                    new CancelQueueTokenRequestDto { Notes = notes });

                TempData["Success"] = "Token cancelled.";
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Queue), new { departmentId, date = date.ToString("yyyy-MM-dd") });
        }
    }
}