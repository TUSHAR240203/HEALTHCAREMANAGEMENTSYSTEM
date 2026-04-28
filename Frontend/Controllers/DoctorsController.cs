using Frontend.Infrastructure;
using Frontend.Models.Doctors;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers
{
    [RequireRole("Admin", "Receptionist", "Doctor")]
    public class DoctorsController : Controller
    {
        private readonly DoctorGatewayService _doctorGatewayService;
        private readonly IWebHostEnvironment _environment;

        public DoctorsController(DoctorGatewayService doctorGatewayService, IWebHostEnvironment environment)
        {
            _doctorGatewayService = doctorGatewayService;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var doctors = await _doctorGatewayService.GetAllAsync();
            return View(doctors);
        }

        [HttpGet]
        [RequireRole("Admin", "Receptionist")]
        public IActionResult Create()
        {
            return View(new CreateDoctorViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole("Admin", "Receptionist")]
        public async Task<IActionResult> Create(CreateDoctorViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            model.PhotoUrl = await SaveDoctorPhotoAsync(model.PhotoFile) ?? model.PhotoUrl;

            var result = await _doctorGatewayService.CreateAsync(model);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["Success"] = "Doctor profile created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var doctor = await _doctorGatewayService.GetByIdAsync(id);
            if (doctor == null)
            {
                TempData["Error"] = "Doctor was not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(doctor);
        }

        [HttpGet]
        public async Task<IActionResult> Leaves(int? doctorId, string? status)
        {
            var role = HttpContext.Session.GetString("Role") ?? string.Empty;
            if (string.Equals(role, "Doctor", StringComparison.OrdinalIgnoreCase) && !doctorId.HasValue)
            {
                return View(new List<DoctorLeaveResponseDto>());
            }

            var leaves = doctorId.HasValue
                ? await _doctorGatewayService.GetLeavesByDoctorAsync(doctorId.Value)
                : await _doctorGatewayService.GetLeavesAsync(status);

            ViewBag.DoctorId = doctorId;
            ViewBag.Status = status;
            return View(leaves);
        }

        [HttpGet]
        [RequireRole("Doctor", "Admin")]
        public IActionResult RequestLeave(int? doctorId)
        {
            return View(new CreateDoctorLeaveViewModel
            {
                DoctorId = doctorId ?? 0,
                LeaveDate = DateOnly.FromDateTime(DateTime.Today)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole("Doctor", "Admin")]
        public async Task<IActionResult> RequestLeave(CreateDoctorLeaveViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                await _doctorGatewayService.RequestLeaveAsync(model);
                TempData["Success"] = "Leave request submitted. Admin approval is required before it affects availability.";
                return RedirectToAction(nameof(Leaves), new { doctorId = model.DoctorId });
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole("Admin")]
        public async Task<IActionResult> ApproveLeave(int leaveId, int? doctorId, string? status)
        {
            try
            {
                await _doctorGatewayService.ApproveLeaveAsync(leaveId, HttpContext.Session.GetString("FullName") ?? "Admin");
                TempData["Success"] = "Doctor leave approved.";
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Leaves), new { doctorId, status });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole("Admin")]
        public async Task<IActionResult> RejectLeave(int leaveId, int? doctorId, string? status)
        {
            try
            {
                await _doctorGatewayService.RejectLeaveAsync(leaveId, HttpContext.Session.GetString("FullName") ?? "Admin");
                TempData["Success"] = "Doctor leave rejected.";
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Leaves), new { doctorId, status });
        }

        private async Task<string?> SaveDoctorPhotoAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(extension))
            {
                ModelState.AddModelError(nameof(CreateDoctorViewModel.PhotoFile), "Only JPG, PNG, or WEBP images are allowed.");
                return null;
            }

            var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", "doctors");
            Directory.CreateDirectory(uploadsRoot);
            var fileName = $"doctor_{Guid.NewGuid():N}{extension}";
            var path = Path.Combine(uploadsRoot, fileName);
            await using var stream = System.IO.File.Create(path);
            await file.CopyToAsync(stream);
            return $"/uploads/doctors/{fileName}";
        }
    }
}
