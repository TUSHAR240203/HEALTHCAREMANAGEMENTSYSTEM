using Frontend.Infrastructure;
using Frontend.Models.Admin;
using Frontend.Models.Doctors;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers
{
    [RequireRole("Admin", "Receptionist", "Doctor")]
    public class DoctorsController : Controller
    {
        private readonly DoctorGatewayService _doctorGatewayService;
        private readonly StaffUserGatewayService _staffUserGatewayService;
        private readonly IWebHostEnvironment _environment;

        public DoctorsController(
            DoctorGatewayService doctorGatewayService,
            StaffUserGatewayService staffUserGatewayService,
            IWebHostEnvironment environment)
        {
            _doctorGatewayService = doctorGatewayService;
            _staffUserGatewayService = staffUserGatewayService;
            _environment = environment;
        }

        [HttpGet]
        [RequireRole("Admin", "Receptionist")]
        public async Task<IActionResult> Index()
        {
            var doctors = await _doctorGatewayService.GetAllAsync();
            return View(doctors);
        }

        [HttpGet]
        [RequireRole("Doctor")]
        public async Task<IActionResult> MyProfile()
        {
            var doctor = await GetLoggedInDoctorAsync();

            if (doctor == null)
            {
                TempData["Error"] = "Your login is not linked to a doctor profile yet. Please ask admin to link your doctor profile.";
                return RedirectToAction("Index", "Home");
            }

            return View("Details", doctor);
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
            if (!ModelState.IsValid)
                return View(model);

            var token = HttpContext.Session.GetString("AccessToken") ?? string.Empty;

            var authResult = await _staffUserGatewayService.CreateAsync(
                token,
                new CreateStaffUserViewModel
                {
                    LoginId = model.LoginId,
                    Password = model.Password,
                    Role = "Doctor",
                    Email = model.Email,
                    MobileNumber = model.Phone,
                    IsActive = model.IsActive,
                    EnablePasswordLogin = model.EnablePasswordLogin,
                    EnableOtpLogin = model.EnableOtpLogin
                });

            if (!authResult.Success || authResult.Data == null)
            {
                ModelState.AddModelError(string.Empty, authResult.Message);
                return View(model);
            }

            model.AuthUserId = authResult.Data.UserId;
            model.PhotoUrl = await SaveDoctorPhotoAsync(model.PhotoFile) ?? model.PhotoUrl;

            var doctorResult = await _doctorGatewayService.CreateAsync(model);

            if (!doctorResult.Success)
            {
                await _staffUserGatewayService.DeleteAsync(token, authResult.Data.UserId);

                ModelState.AddModelError(string.Empty,
                    $"Login account was created but doctor profile failed: {doctorResult.Message}. The login account was removed. Please try again.");

                return View(model);
            }

            TempData["Success"] = "Doctor login and doctor profile created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [RequireRole("Admin", "Receptionist")]
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
        [RequireRole("Doctor")]
        public async Task<IActionResult> MySchedule()
        {
            var doctor = await GetLoggedInDoctorAsync();

            if (doctor == null)
            {
                TempData["Error"] = "Your login is not linked to a doctor profile yet. Please ask admin to link your doctor profile.";
                return RedirectToAction("Index", "Home");
            }

            var schedules = await _doctorGatewayService.GetSchedulesAsync(doctor.Id);

            ViewBag.DoctorId = doctor.Id;
            ViewBag.DoctorName = doctor.FullName;

            return View(schedules);
        }

        [HttpGet]
        [RequireRole("Doctor")]
        public IActionResult AddMySchedule()
        {
            return View(new CreateDoctorScheduleViewModel
            {
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(17, 0),
                BreakStartTime = new TimeOnly(13, 0),
                BreakEndTime = new TimeOnly(14, 0),
                SlotDurationMinutes = 30,
                IsActive = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole("Doctor")]
        public async Task<IActionResult> AddMySchedule(CreateDoctorScheduleViewModel model)
        {
            ValidateSchedule(model);

            if (!ModelState.IsValid)
                return View(model);

            var doctor = await GetLoggedInDoctorAsync();

            if (doctor == null)
            {
                ModelState.AddModelError(string.Empty, "Your login is not linked to a doctor profile yet.");
                return View(model);
            }

            try
            {
                await _doctorGatewayService.AddScheduleAsync(doctor.Id, model);

                TempData["Success"] = "Schedule added successfully.";
                return RedirectToAction(nameof(MySchedule));
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole("Doctor")]
        public async Task<IActionResult> DeleteMySchedule(int scheduleId)
        {
            var doctor = await GetLoggedInDoctorAsync();

            if (doctor == null)
            {
                TempData["Error"] = "Your login is not linked to a doctor profile yet.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                await _doctorGatewayService.DeleteScheduleAsync(doctor.Id, scheduleId);
                TempData["Success"] = "Schedule deleted successfully.";
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(MySchedule));
        }

        [HttpGet]
        public async Task<IActionResult> Leaves(int? doctorId, string? status)
        {
            var role = HttpContext.Session.GetString("Role") ?? string.Empty;

            if (string.Equals(role, "Doctor", StringComparison.OrdinalIgnoreCase))
            {
                var doctor = await GetLoggedInDoctorAsync();

                if (doctor == null)
                {
                    TempData["Error"] = "Your login is not linked to a doctor profile yet. Please ask admin to link your doctor profile.";
                    return RedirectToAction("Index", "Home");
                }

                var ownLeaves = await _doctorGatewayService.GetLeavesByDoctorAsync(doctor.Id);

                ViewBag.DoctorId = doctor.Id;
                ViewBag.Status = null;
                ViewBag.IsDoctorOwnLeavePage = true;

                return View(ownLeaves);
            }

            var leaves = doctorId.HasValue
                ? await _doctorGatewayService.GetLeavesByDoctorAsync(doctorId.Value)
                : await _doctorGatewayService.GetLeavesAsync(status);

            ViewBag.DoctorId = doctorId;
            ViewBag.Status = status;
            ViewBag.IsDoctorOwnLeavePage = false;

            return View(leaves);
        }

        [HttpGet]
        [RequireRole("Doctor")]
        public IActionResult RequestLeave()
        {
            return View(new CreateDoctorLeaveViewModel
            {
                StartDate = DateOnly.FromDateTime(DateTime.Today),
                EndDate = DateOnly.FromDateTime(DateTime.Today)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole("Doctor")]
        public async Task<IActionResult> RequestLeave(CreateDoctorLeaveViewModel model)
        {
            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError(nameof(model.EndDate),
                    "End date must be greater than or equal to start date.");
            }

            if (!ModelState.IsValid)
                return View(model);

            var doctor = await GetLoggedInDoctorAsync();

            if (doctor == null)
            {
                ModelState.AddModelError(string.Empty,
                    "Your login is not linked to a doctor profile yet. Please ask admin to link your doctor profile.");
                return View(model);
            }

            try
            {
                await _doctorGatewayService.RequestLeaveAsync(doctor.Id, model);

                TempData["Success"] = "Leave request submitted. Admin approval is required before it affects availability.";
                return RedirectToAction(nameof(Leaves));
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
                await _doctorGatewayService.ApproveLeaveAsync(
                    leaveId,
                    HttpContext.Session.GetString("FullName") ?? "Admin");

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
                await _doctorGatewayService.RejectLeaveAsync(
                    leaveId,
                    HttpContext.Session.GetString("FullName") ?? "Admin");

                TempData["Success"] = "Doctor leave rejected.";
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Leaves), new { doctorId, status });
        }

        private async Task<DoctorResponseDto?> GetLoggedInDoctorAsync()
        {
            var authUserId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (authUserId <= 0)
                return null;

            return await _doctorGatewayService.GetByAuthUserIdAsync(authUserId);
        }

        private void ValidateSchedule(CreateDoctorScheduleViewModel model)
        {
            if (model.EndTime <= model.StartTime)
            {
                ModelState.AddModelError(nameof(model.EndTime),
                    "End time must be after start time.");
            }

            if (model.BreakStartTime.HasValue != model.BreakEndTime.HasValue)
            {
                ModelState.AddModelError(nameof(model.BreakStartTime),
                    "Both break start time and break end time are required.");
            }

            if (model.BreakStartTime.HasValue && model.BreakEndTime.HasValue)
            {
                if (model.BreakEndTime <= model.BreakStartTime)
                {
                    ModelState.AddModelError(nameof(model.BreakEndTime),
                        "Break end time must be after break start time.");
                }

                if (model.BreakStartTime < model.StartTime || model.BreakEndTime > model.EndTime)
                {
                    ModelState.AddModelError(nameof(model.BreakStartTime),
                        "Break time must be within working hours.");
                }
            }
        }

        private async Task<string?> SaveDoctorPhotoAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;

            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowed.Contains(extension))
            {
                ModelState.AddModelError(nameof(CreateDoctorViewModel.PhotoFile),
                    "Only JPG, PNG, or WEBP images are allowed.");

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