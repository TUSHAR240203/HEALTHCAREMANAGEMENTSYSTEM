using Frontend.Models.Patients;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using Frontend.Infrastructure;

namespace Frontend.Controllers
{
    [RequireRole("Admin", "Receptionist")]
    public class PatientsController : Controller
    {
        private readonly PatientGatewayService _patientGatewayService;

        public PatientsController(PatientGatewayService patientGatewayService)
        {
            _patientGatewayService = patientGatewayService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? query, string? uhid, string? mobileNumber, int page = 1)
        {
            const int pageSize = 5;
            page = Math.Max(1, page);

            var model = new PatientSearchViewModel
            {
                Query = query,
                UHID = uhid,
                MobileNumber = mobileNumber,
                Results = new List<PatientResponseDto>()
            };

            try
            {
                var request = new PatientSearchRequestDto
                {
                    Query = string.IsNullOrWhiteSpace(query) ? null : query.Trim(),
                    UHID = string.IsNullOrWhiteSpace(uhid) ? null : uhid.Trim(),
                    MobileNumber = string.IsNullOrWhiteSpace(mobileNumber) ? null : mobileNumber.Trim()
                };

                var patients = await _patientGatewayService.SearchAsync(request)
                    ?? new List<PatientResponseDto>();

                foreach (var patient in patients)
                {
                    NormalizePatientId(patient);
                }

                var totalItems = patients.Count;
                var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));

                if (page > totalPages)
                {
                    page = totalPages;
                }

                model.Results = patients
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.HasSearched =
                    !string.IsNullOrWhiteSpace(query) ||
                    !string.IsNullOrWhiteSpace(uhid) ||
                    !string.IsNullOrWhiteSpace(mobileNumber);
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;

                return View(model);
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
                model.Results = new List<PatientResponseDto>();
                ViewBag.HasSearched = false;
                ViewBag.CurrentPage = 1;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = 1;
                ViewBag.TotalItems = 0;

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(PatientSearchViewModel model)
        {
            return RedirectToAction(nameof(Index), new
            {
                query = model.Query,
                uhid = model.UHID,
                mobileNumber = model.MobileNumber,
                page = 1
            });
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreatePatientRequestDto
            {
                DateOfBirth = DateOnly.FromDateTime(DateTime.Today)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePatientRequestDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var result = await _patientGatewayService.CreateAsync(model);

                if (!result.Success)
                {
                    ModelState.AddModelError(string.Empty, result.Message);
                    return View(model);
                }

                TempData["Success"] = result.Message;

                if (result.Data != null)
                {
                    NormalizePatientId(result.Data);

                    var patientId = result.Data.EffectivePatientId;

                    if (patientId > 0)
                    {
                        return RedirectToAction(nameof(Details), new
                        {
                            id = patientId
                        });
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "Invalid patient id.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var patient = await _patientGatewayService.GetByIdAsync(id);

                if (patient == null)
                {
                    TempData["Error"] = "Patient not found.";
                    return RedirectToAction(nameof(Index));
                }

                NormalizePatientId(patient);

                return View(patient);
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "Invalid patient id.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var patient = await _patientGatewayService.GetByIdAsync(id);

                if (patient == null)
                {
                    TempData["Error"] = "Patient not found.";
                    return RedirectToAction(nameof(Index));
                }

                NormalizePatientId(patient);

                var patientId = patient.EffectivePatientId;
                var names = patient.FullName?.Split(' ', 2);

                var model = new UpdatePatientRequestDto
                {
                    FirstName = names?.Length > 0 ? names[0] : "",
                    LastName = names?.Length > 1 ? names[1] : "",
                    DateOfBirth = patient.DateOfBirth,
                    Gender = patient.Gender,
                    MobileNumber = patient.MobileNumber,
                    Email = patient.Email,
                    BloodGroup = patient.BloodGroup,
                    PortalAccessEnabled = patient.PortalAccessEnabled,
                    PortalActivated = patient.PortalActivated,
                    Status = patient.Status
                };

                ViewBag.PatientId = patientId;
                ViewBag.Uhid = patient.DisplayUHID;

                return View(model);
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdatePatientRequestDto model)
        {
            if (id <= 0)
            {
                TempData["Error"] = "Invalid patient id.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                ViewBag.PatientId = id;
                return View(model);
            }

            try
            {
                var result = await _patientGatewayService.UpdateAsync(id, model);

                if (!result.Success)
                {
                    ModelState.AddModelError(string.Empty, result.Message);
                    ViewBag.PatientId = id;
                    return View(model);
                }

                TempData["Success"] = result.Message;

                return RedirectToAction(nameof(Details), new
                {
                    id
                });
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.PatientId = id;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "Invalid patient id.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var patient = await _patientGatewayService.GetByIdAsync(id);

                if (patient == null)
                {
                    TempData["Error"] = "Patient not found.";
                    return RedirectToAction(nameof(Index));
                }

                NormalizePatientId(patient);

                return View(patient);
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "Invalid patient id.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var result = await _patientGatewayService.DeleteAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;

                    return RedirectToAction(nameof(Details), new
                    {
                        id
                    });
                }

                TempData["Success"] = result.Message;

                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;

                return RedirectToAction(nameof(Details), new
                {
                    id
                });
            }
        }

        private static void NormalizePatientId(PatientResponseDto patient)
        {
            var effectiveId = patient.EffectivePatientId;

            if (effectiveId > 0)
            {
                patient.Id = effectiveId;
                patient.PatientId = effectiveId;
            }
        }
    }
    [RequireRole("Patient")]
    public class PatientPortalController : Controller
    {
        [HttpGet]
        public IActionResult Dashboard()
        {
            var role = HttpContext.Session.GetString("Role");
            var patientId = HttpContext.Session.GetInt32("PatientId") ?? 0;

            if (!string.Equals(role, "Patient", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Please login as patient.";
                return RedirectToAction("Login", "Account");
            }

            if (patientId <= 0)
            {
                TempData["Error"] = "Patient session expired. Please login again.";
                return RedirectToAction("Login", "Account");
            }

            return View();
        }
    }
    }