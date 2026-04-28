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
        public IActionResult Index()
        {
            return View(new PatientSearchViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(PatientSearchViewModel model)
        {
            var request = new PatientSearchRequestDto
            {
                Query = model.Query,
                UHID = model.UHID,
                MobileNumber = model.MobileNumber
            };

            model.Results = await _patientGatewayService.SearchAsync(request);
            return View(model);
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

            var result = await _patientGatewayService.CreateAsync(model);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["Success"] = result.Message;

            if (result.Data != null)
                return RedirectToAction(nameof(Details), new { id = result.Data.Id });

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var patient = await _patientGatewayService.GetByIdAsync(id);

            if (patient == null)
            {
                TempData["Error"] = "Patient not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(patient);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var patient = await _patientGatewayService.GetByIdAsync(id);

            if (patient == null)
            {
                TempData["Error"] = "Patient not found.";
                return RedirectToAction(nameof(Index));
            }

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

            ViewBag.PatientId = patient.Id;
            ViewBag.Uhid = patient.Uhid;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdatePatientRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.PatientId = id;
                return View(model);
            }

            var result = await _patientGatewayService.UpdateAsync(id, model);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                ViewBag.PatientId = id;
                return View(model);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var patient = await _patientGatewayService.GetByIdAsync(id);

            if (patient == null)
            {
                TempData["Error"] = "Patient not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(patient);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _patientGatewayService.DeleteAsync(id);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}