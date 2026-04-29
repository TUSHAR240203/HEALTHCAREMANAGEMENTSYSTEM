using Frontend.Models.Reception;
using Frontend.Models.Api;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Frontend.Infrastructure;

namespace Frontend.Controllers
{
    [RequireRole("Admin", "Receptionist")]
    public class ReceptionController : Controller
    {
        private readonly IReceptionApiService _receptionApiService;
        private readonly IAppointmentApiService _appointmentApiService;

        public ReceptionController(
            IReceptionApiService receptionApiService,
            IAppointmentApiService appointmentApiService)
        {
            _receptionApiService = receptionApiService;
            _appointmentApiService = appointmentApiService;
        }

        [HttpGet]
        public async Task<IActionResult> SearchPatients()
        {
            var request = new ReceptionPatientSearchRequestDto();

            try
            {
                var result = await _receptionApiService.SearchPatientsAsync(request);

                ViewBag.Results = result?.Patients ?? new List<ReceptionPatientSummaryDto>();
                ViewBag.HasSearched = false;

                return View(request);
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
                ViewBag.Results = new List<ReceptionPatientSummaryDto>();
                ViewBag.HasSearched = false;

                return View(request);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SearchPatients(ReceptionPatientSearchRequestDto request)
        {
            try
            {
                var result = await _receptionApiService.SearchPatientsAsync(request);

                ViewBag.Results = result?.Patients ?? new List<ReceptionPatientSummaryDto>();
                ViewBag.HasSearched = true;

                return View(request);
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                ViewBag.Results = new List<ReceptionPatientSummaryDto>();
                ViewBag.HasSearched = true;

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
            if (patientId <= 0)
            {
                TempData["Error"] = "Invalid patient id.";
                return RedirectToAction(nameof(SearchPatients));
            }

            try
            {
                var result = await _receptionApiService.GetPatientSummaryAsync(patientId);

                if (result == null)
                {
                    TempData["Error"] = "Patient not found.";
                    return RedirectToAction(nameof(SearchPatients));
                }

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

        // FINAL CHECK-IN GET
        // This loads today's scheduled appointments into the searchable dropdown.
        [HttpGet]
        public async Task<IActionResult> CheckIn(DateOnly? date)
        {
            var checkInDate = date ?? DateOnly.FromDateTime(DateTime.Today);

            try
            {
                var appointments =
                    await _receptionApiService.GetTodayScheduledAppointmentsForCheckInAsync(checkInDate);

                var model = new CheckInPageViewModel
                {
                    Date = checkInDate,
                    Appointments = appointments,
                    CheckIn = new CheckInRequestDto
                    {
                        CheckInTimeUtc = DateTime.UtcNow
                    }
                };

                return View(model);
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;

                return View(new CheckInPageViewModel
                {
                    Date = checkInDate,
                    Appointments = new List<TodayAppointmentForCheckInDto>(),
                    CheckIn = new CheckInRequestDto
                    {
                        CheckInTimeUtc = DateTime.UtcNow
                    }
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(CheckInPageViewModel model)
        {
            if (model.Date == default)
            {
                model.Date = DateOnly.FromDateTime(DateTime.Today);
            }

            if (model.CheckIn.AppointmentId <= 0)
            {
                ModelState.AddModelError(string.Empty, "Please select an appointment for check-in.");
            }

            if (model.CheckIn.PatientId <= 0)
            {
                ModelState.AddModelError(string.Empty, "Patient details are missing. Select an appointment again.");
            }

            if (model.CheckIn.DoctorId <= 0)
            {
                ModelState.AddModelError(string.Empty, "Doctor details are missing. Select an appointment again.");
            }

            if (model.CheckIn.DepartmentId <= 0)
            {
                ModelState.AddModelError(string.Empty, "Department details are missing. Select an appointment again.");
            }

            if (!ModelState.IsValid)
            {
                model.Appointments =
                    await _receptionApiService.GetTodayScheduledAppointmentsForCheckInAsync(model.Date);

                return View(model);
            }

            try
            {
                model.CheckIn.QueueDate = model.Date;
                model.CheckIn.CheckInTimeUtc = DateTime.UtcNow;

                await _receptionApiService.CheckInAsync<object>(model.CheckIn);

                TempData["Success"] = "Patient checked in successfully and added to queue.";

                return RedirectToAction(nameof(Queue), new
                {
                    departmentId = model.CheckIn.DepartmentId,
                    date = model.CheckIn.QueueDate.ToString("yyyy-MM-dd")
                });
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                model.Appointments =
                    await _receptionApiService.GetTodayScheduledAppointmentsForCheckInAsync(model.Date);

                return View(model);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Queue(int departmentId = 1, DateOnly? date = null)
        {
            var queueDate = date ?? DateOnly.FromDateTime(DateTime.Today);

            try
            {
                var result = await _receptionApiService.GetQueueAsync(departmentId, queueDate);

                ViewBag.CurrentQueue =
                    await _receptionApiService.GetCurrentQueueAsync(departmentId, queueDate);

                return View(result ?? new DepartmentQueueResponseDto
                {
                    DepartmentId = departmentId,
                    DepartmentName = $"Department {departmentId}",
                    Date = queueDate,
                    Queue = new List<QueueItemDto>()
                });
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;

                return View(new DepartmentQueueResponseDto
                {
                    DepartmentId = departmentId,
                    DepartmentName = $"Department {departmentId}",
                    Date = queueDate,
                    Queue = new List<QueueItemDto>()
                });
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

            return RedirectToAction(nameof(Queue), new
            {
                departmentId,
                date = date.ToString("yyyy-MM-dd")
            });
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

            return RedirectToAction(nameof(Queue), new
            {
                departmentId,
                date = date.ToString("yyyy-MM-dd")
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteToken(
            int queueTokenId,
            int departmentId,
            DateOnly date,
            int appointmentId,
            int patientId,
            string? notes)
        {
            try
            {
                await _receptionApiService.CompleteTokenAsync<object>(
                    queueTokenId,
                    new CompleteQueueTokenRequestDto
                    {
                        Notes = notes
                    });

                var resolvedAppointmentId = appointmentId > 0
                    ? appointmentId
                    : await ResolveAppointmentIdForQueueCompletionAsync(patientId, departmentId, date);

                if (resolvedAppointmentId > 0)
                {
                    await _appointmentApiService.CompleteAsync(
                        resolvedAppointmentId,
                        new CompleteAppointmentRequestDto
                        {
                            Notes = notes ?? "Completed from queue"
                        });
                }

                TempData["Success"] = resolvedAppointmentId > 0
                    ? "Patient completed and appointment status updated in database."
                    : "Patient completed in queue, but appointment status could not be updated because AppointmentId was not found. Run the SQL sync script once or expose AppointmentId from the Reception API queue response.";
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Queue), new
            {
                departmentId,
                date = date.ToString("yyyy-MM-dd")
            });
        }

        private async Task<int> ResolveAppointmentIdForQueueCompletionAsync(
            int patientId,
            int departmentId,
            DateOnly date)
        {
            if (patientId <= 0 || departmentId <= 0)
            {
                return 0;
            }

            var result = await _appointmentApiService.SearchAsync(new AppointmentSearchRequestDto
            {
                PatientId = patientId,
                DepartmentId = departmentId,
                AppointmentDate = date,
                PageNumber = 1,
                PageSize = 20
            });

            var appointment = result.Appointments
                .Where(x => x.Status != AppointmentStatus.Completed &&
                            x.Status != AppointmentStatus.Cancelled)
                .OrderByDescending(x => x.Status == AppointmentStatus.CheckedIn)
                .ThenBy(x => x.SlotStartTime)
                .FirstOrDefault();

            return appointment?.Id ?? 0;
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

            return RedirectToAction(nameof(Queue), new
            {
                departmentId,
                date = date.ToString("yyyy-MM-dd")
            });
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

            return RedirectToAction(nameof(Queue), new
            {
                departmentId,
                date = date.ToString("yyyy-MM-dd")
            });
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
                    new CancelQueueTokenRequestDto
                    {
                        Notes = notes
                    });

                TempData["Success"] = "Token cancelled.";
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Queue), new
            {
                departmentId,
                date = date.ToString("yyyy-MM-dd")
            });
        }
    }
}