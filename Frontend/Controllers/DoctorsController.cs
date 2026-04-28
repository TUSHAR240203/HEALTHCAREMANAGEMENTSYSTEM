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
