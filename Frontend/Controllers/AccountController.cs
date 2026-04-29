using Frontend.Models.Auth;
using Frontend.Models.Patients;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthGatewayService _authGatewayService;
        private readonly PatientGatewayService _patientGatewayService;
        private readonly DoctorGatewayService _doctorGatewayService;
        private readonly IWebHostEnvironment _environment;

        public AccountController(
    AuthGatewayService authGatewayService,
    PatientGatewayService patientGatewayService,
    DoctorGatewayService doctorGatewayService,
    IWebHostEnvironment environment)
        {
            _authGatewayService = authGatewayService;
            _patientGatewayService = patientGatewayService;
            _doctorGatewayService = doctorGatewayService;
            _environment = environment;
        }

        [HttpGet]
        public IActionResult StaffLogin() => View(new StaffLoginViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StaffLogin(StaffLoginViewModel model, string submitButton)
        {
            (bool Success, AuthResponseDto? Data, string Message) result;

            if (submitButton == "sendOtp")
            {
                if (string.IsNullOrWhiteSpace(model.LoginId) && string.IsNullOrWhiteSpace(model.MobileNumber))
                {
                    ModelState.AddModelError(string.Empty, "Login ID or mobile number is required.");
                    return View(model);
                }

                var otpResult = await _authGatewayService.SendStaffLoginOtpAsync(!string.IsNullOrWhiteSpace(model.LoginId) ? model.LoginId : model.MobileNumber);
                if (!otpResult.Success)
                {
                    ModelState.AddModelError(string.Empty, otpResult.Message);
                    return View(model);
                }

                TempData["Success"] = "OTP sent successfully. Check the Auth API console output.";
                model.OtpSent = true;
                return View(model);
            }

            if (submitButton == "verifyOtp")
            {
                if (string.IsNullOrWhiteSpace(model.OtpCode) || (string.IsNullOrWhiteSpace(model.LoginId) && string.IsNullOrWhiteSpace(model.MobileNumber)))
                {
                    ModelState.AddModelError(string.Empty, "Login ID/mobile number and OTP are required.");
                    model.OtpSent = true;
                    return View(model);
                }

                result = await _authGatewayService.StaffOtpLoginAsync(new StaffOtpLoginRequestDto { LoginId = model.LoginId, MobileNumber = model.MobileNumber, OtpCode = model.OtpCode });
            }
            else
            {
                if (string.IsNullOrWhiteSpace(model.LoginId) || string.IsNullOrWhiteSpace(model.Password))
                {
                    ModelState.AddModelError(string.Empty, "Login ID and password are required.");
                    return View(model);
                }

                result = await _authGatewayService.StaffLoginAsync(new StaffLoginRequestDto { LoginId = model.LoginId, Password = model.Password });
            }

            if (!result.Success || result.Data == null)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                model.OtpSent = submitButton == "verifyOtp";
                return View(model);
            }

            await SetAuthSessionAsync(result.Data);
            TempData["Success"] = "Login successful.";

            if (!result.Data.IsFirstLoginCompleted)
                return RedirectToAction(nameof(AuthPreference));

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AuthPreference()
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString("AccessToken")))
                return RedirectToAction(nameof(StaffLogin));

            var role = HttpContext.Session.GetString("Role");
            var isPatient = string.Equals(role, "Patient", StringComparison.OrdinalIgnoreCase);
            return View(new AuthPreferenceViewModel { EnableOtpLogin = true, EnablePasswordLogin = !isPatient });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AuthPreference(AuthPreferenceViewModel model)
        {
            var token = HttpContext.Session.GetString("AccessToken");
            if (string.IsNullOrWhiteSpace(token)) return RedirectToAction(nameof(StaffLogin));

            var role = HttpContext.Session.GetString("Role");
            var isPatient = string.Equals(role, "Patient", StringComparison.OrdinalIgnoreCase);
            if (isPatient)
            {
                model.EnableOtpLogin = true;
                model.EnablePasswordLogin = false;
                model.LoginId = null;
                model.Password = null;
            }

            if (!model.EnableOtpLogin && !model.EnablePasswordLogin)
            {
                ModelState.AddModelError(string.Empty, "Select at least one login method.");
                return View(model);
            }

            var result = await _authGatewayService.UpdateAuthPreferenceAsync(token, model);
            if (!result.Success || result.Data == null)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            await SetAuthSessionAsync(result.Data);
            TempData["Success"] = "Authentication preference saved.";
            return string.Equals(result.Data.Role, "Patient", StringComparison.OrdinalIgnoreCase)
                ? RedirectToAction(nameof(CompleteProfile))
                : RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new CreatePatientRequestDto
            {
                DateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)),
                PortalAccessEnabled = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(CreatePatientRequestDto model)
        {
            model.PortalAccessEnabled = true;

            if (!ModelState.IsValid)
                return View(model);

            var result = await _patientGatewayService.RegisterForPortalAsync(model);
            if (!result.Success || result.Data == null)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["Success"] = $"Registration successful. Your Patient ID is {result.Data.Id}. Use it with your mobile number to login by OTP.";
            return RedirectToAction(nameof(Login), new { patientId = result.Data.Id, mobileNumber = result.Data.MobileNumber });
        }

        [HttpGet]
        public IActionResult Login(int? patientId, string? mobileNumber)
        {
            return View(new PatientLoginViewModel
            {
                PatientId = patientId ?? 0,
                MobileNumber = mobileNumber ?? string.Empty
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(PatientLoginViewModel model, string submitButton)
        {
            if (submitButton == "sendOtp")
            {
                if (model.PatientId <= 0 || string.IsNullOrWhiteSpace(model.MobileNumber))
                {
                    ModelState.AddModelError(string.Empty, "Patient ID and Mobile Number are required.");
                    return View(model);
                }

                var result = await _authGatewayService.SendLoginOtpAsync(model.PatientId, model.MobileNumber);
                if (!result.Success)
                {
                    ModelState.AddModelError(string.Empty, result.Message);
                    model.OtpSent = false;
                    return View(model);
                }

                TempData["Success"] = "OTP sent successfully. Check the Auth API console output.";
                model.OtpSent = true;
                return View(model);
            }

            if (submitButton == "verifyOtp")
            {
                if (model.PatientId <= 0 || string.IsNullOrWhiteSpace(model.MobileNumber) || string.IsNullOrWhiteSpace(model.OtpCode))
                {
                    ModelState.AddModelError(string.Empty, "Patient ID, Mobile Number, and OTP are required.");
                    model.OtpSent = true;
                    return View(model);
                }

                var request = new PatientLoginRequestDto
                {
                    PatientId = model.PatientId,
                    MobileNumber = model.MobileNumber,
                    OtpCode = model.OtpCode
                };

                var result = await _authGatewayService.LoginAsync(request);

                if (!result.Success || result.Data == null)
                {
                    ModelState.AddModelError(string.Empty, result.Message);
                    model.OtpSent = true;
                    return View(model);
                }

                await SetAuthSessionAsync(result.Data);
                TempData["Success"] = "Login successful.";

                if (!result.Data.IsFirstLoginCompleted) return RedirectToAction(nameof(AuthPreference));
                return RedirectToAction(nameof(CompleteProfile));
            }

            ModelState.AddModelError(string.Empty, "Invalid action.");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CompleteProfile()
        {
            var patientId = HttpContext.Session.GetInt32("PatientId") ?? 0;
            if (patientId <= 0) return RedirectToAction(nameof(Login));

            var patient = await _patientGatewayService.GetByIdAsync(patientId);
            if (patient == null)
            {
                TempData["Error"] = "Patient profile could not be loaded.";
                return RedirectToAction("Index", "Home");
            }

            var model = BuildCompleteProfileViewModel(patient);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteProfile(CompletePatientProfileViewModel model)
        {
            var patientId = HttpContext.Session.GetInt32("PatientId") ?? 0;
            if (patientId <= 0) return RedirectToAction(nameof(Login));

            model.PatientId = patientId;
            if (!ModelState.IsValid)
            {
                model.CompletionPercentage = CalculateProfileCompletion(model);
                return View(model);
            }

            // Handle photo upload from gallery (takes priority over URL)
            if (model.PhotoFile != null && model.PhotoFile.Length > 0)
            {
                var saved = await SavePatientPhotoAsync(model.PhotoFile, patientId);

                if (saved != null) model.PhotoUrl = saved;
            }

            var request = new CompletePatientProfileRequestDto
            {
                BloodGroup = model.BloodGroup,
                MaritalStatus = model.MaritalStatus,
                AddressLine1 = model.AddressLine1,
                AddressLine2 = model.AddressLine2,
                City = model.City,
                State = model.State,
                PostalCode = model.PostalCode,
                EmergencyContactName = model.EmergencyContactName,
                EmergencyContactNumber = model.EmergencyContactNumber,
                EmergencyContactRelation = model.EmergencyContactRelation,
                AadhaarNumber = model.AadhaarNumber,
                InsuranceProvider = model.InsuranceProvider,
                InsurancePolicyNumber = model.InsurancePolicyNumber,
                PhotoUrl = model.PhotoUrl
            };

            var result = await _patientGatewayService.CompleteProfileAsync(patientId, request);
            if (!result.Success || result.Data == null)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                model.CompletionPercentage = CalculateProfileCompletion(model);
                return View(model);
            }

            RefreshPatientSession(result.Data);
            TempData["Success"] = "Profile saved successfully.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var token = HttpContext.Session.GetString("AccessToken");
            if (string.IsNullOrWhiteSpace(token)) return RedirectToAction(nameof(StaffLogin));

            var user = await _authGatewayService.GetCurrentUserAsync(token);
            if (user == null)
            {
                TempData["Error"] = "Session expired or unauthorized.";
                return RedirectToAction(nameof(Login));
            }

            if (string.IsNullOrWhiteSpace(user.PhotoUrl))
                user.PhotoUrl = HttpContext.Session.GetString("PhotoUrl");
            else
                HttpContext.Session.SetString("PhotoUrl", user.PhotoUrl);

            user.IsProfileCompleted = string.Equals(HttpContext.Session.GetString("IsProfileCompleted"), "true", StringComparison.OrdinalIgnoreCase);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadStaffPhoto(IFormFile photo)
        {
            var token = HttpContext.Session.GetString("AccessToken");
            if (string.IsNullOrWhiteSpace(token)) return RedirectToAction(nameof(StaffLogin));

            var role = HttpContext.Session.GetString("Role");
            var savedPhotoUrl = await SaveStaffPhotoAsync(photo, role);
            if (string.IsNullOrWhiteSpace(savedPhotoUrl))
            {
                TempData["Error"] = "Please upload a valid JPG, PNG, or WEBP photo up to 2 MB.";
                return RedirectToAction(nameof(Profile));
            }

            var result = await _authGatewayService.UpdateMyPhotoUrlAsync(token, savedPhotoUrl);
            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Profile));
            }

            HttpContext.Session.SetString("PhotoUrl", result.Data.PhotoUrl ?? savedPhotoUrl);
            TempData["Success"] = "Profile photo uploaded successfully.";
            return RedirectToAction(nameof(Profile));
        }


        private async Task SetAuthSessionAsync(AuthResponseDto data)
        {
            HttpContext.Session.SetString("AccessToken", data.AccessToken);
            HttpContext.Session.SetString("Role", data.Role);
            HttpContext.Session.SetString("MobileNumber", data.MobileNumber ?? string.Empty);
            HttpContext.Session.SetInt32("UserId", data.UserId);
            HttpContext.Session.SetString("IsProfileCompleted", data.IsProfileCompleted ? "true" : "false");

            if (!string.IsNullOrWhiteSpace(data.UHID))
                HttpContext.Session.SetString("UHID", data.UHID);
            else
                HttpContext.Session.Remove("UHID");

            if (!string.IsNullOrWhiteSpace(data.FullName))
                HttpContext.Session.SetString("FullName", data.FullName);
            else if (!string.IsNullOrWhiteSpace(data.MobileNumber))
                HttpContext.Session.SetString("FullName", data.MobileNumber);

            if (!string.IsNullOrWhiteSpace(data.PhotoUrl))
                HttpContext.Session.SetString("PhotoUrl", data.PhotoUrl);
            else
                HttpContext.Session.Remove("PhotoUrl");

            if (data.PatientId > 0)
            {
                HttpContext.Session.SetInt32("PatientId", data.PatientId);

                var patient = await _patientGatewayService.GetByIdAsync(data.PatientId);
                if (patient != null)
                    RefreshPatientSession(patient);
            }

            if (string.Equals(data.Role, "Doctor", StringComparison.OrdinalIgnoreCase))
            {
                var doctor = await _doctorGatewayService.GetByAuthUserIdAsync(data.UserId);

                if (doctor != null)
                {
                    HttpContext.Session.SetInt32("DoctorId", doctor.Id);
                    HttpContext.Session.SetString("FullName", doctor.FullName);
                    HttpContext.Session.SetString("MobileNumber", doctor.Phone ?? string.Empty);

                    if (!string.IsNullOrWhiteSpace(doctor.PhotoUrl))
                        HttpContext.Session.SetString("PhotoUrl", doctor.PhotoUrl);
                    else
                        HttpContext.Session.Remove("PhotoUrl");
                }
                else
                {
                    HttpContext.Session.Remove("DoctorId");
                }
            }
        }

        private void RefreshPatientSession(PatientResponseDto patient)
        {
            HttpContext.Session.SetInt32("PatientId", patient.Id);
            if (!string.IsNullOrWhiteSpace(patient.Uhid)) HttpContext.Session.SetString("UHID", patient.Uhid);
            if (!string.IsNullOrWhiteSpace(patient.FullName)) HttpContext.Session.SetString("FullName", patient.FullName);
            if (!string.IsNullOrWhiteSpace(patient.PhotoUrl)) HttpContext.Session.SetString("PhotoUrl", patient.PhotoUrl!);
            else HttpContext.Session.Remove("PhotoUrl");
            HttpContext.Session.SetString("IsProfileCompleted", patient.IsProfileCompleted ? "true" : "false");
            HttpContext.Session.SetInt32("ProfileCompletion", CalculateProfileCompletion(patient));
        }

        private static CompletePatientProfileViewModel BuildCompleteProfileViewModel(PatientResponseDto patient)
        {
            return new CompletePatientProfileViewModel
            {
                PatientId = patient.Id,
                FullName = patient.FullName,
                UHID = patient.Uhid,
                MobileNumber = patient.MobileNumber,
                Email = patient.Email,
                IsProfileCompleted = patient.IsProfileCompleted,
                PhotoUrl = patient.PhotoUrl,
                BloodGroup = patient.BloodGroup,
                MaritalStatus = patient.MaritalStatus,
                AddressLine1 = patient.AddressLine1,
                AddressLine2 = patient.AddressLine2,
                City = patient.City,
                State = patient.State,
                PostalCode = patient.PostalCode,
                EmergencyContactName = patient.EmergencyContactName,
                EmergencyContactNumber = patient.EmergencyContactNumber,
                EmergencyContactRelation = patient.EmergencyContactRelation,
                AadhaarNumber = patient.AadhaarNumber,
                InsuranceProvider = patient.InsuranceProvider,
                InsurancePolicyNumber = patient.InsurancePolicyNumber,
                CompletionPercentage = CalculateProfileCompletion(patient)
            };
        }

        private static int CalculateProfileCompletion(PatientResponseDto patient)
        {
            var filled = new[]
            {
                patient.FullName, patient.MobileNumber, patient.Email, patient.BloodGroup, patient.PhotoUrl,
                patient.MaritalStatus, patient.AddressLine1, patient.City, patient.State, patient.PostalCode,
                patient.EmergencyContactName, patient.EmergencyContactNumber, patient.EmergencyContactRelation,
                patient.AadhaarNumber, patient.InsuranceProvider, patient.InsurancePolicyNumber
            }.Count(v => !string.IsNullOrWhiteSpace(v));

            return Math.Clamp((int)Math.Round(filled / 16.0 * 100), 15, 100);
        }

        private static int CalculateProfileCompletion(CompletePatientProfileViewModel model)
        {
            var filled = new[]
            {
                model.FullName, model.MobileNumber, model.Email, model.BloodGroup, model.PhotoUrl,
                model.MaritalStatus, model.AddressLine1, model.City, model.State, model.PostalCode,
                model.EmergencyContactName, model.EmergencyContactNumber, model.EmergencyContactRelation,
                model.AadhaarNumber, model.InsuranceProvider, model.InsurancePolicyNumber
            }.Count(v => !string.IsNullOrWhiteSpace(v));

            return Math.Clamp((int)Math.Round(filled / 16.0 * 100), 15, 100);
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "Logged out successfully.";
            return RedirectToAction(nameof(Login));
        }

        private async Task<string?> SaveStaffPhotoAsync(IFormFile? file, string? role)
        {
            if (file == null || file.Length == 0 || file.Length > 2 * 1024 * 1024) return null;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowed.Contains(extension)) return null;

            var folder = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
                ? "admins"
                : string.Equals(role, "Receptionist", StringComparison.OrdinalIgnoreCase)
                    ? "receptionists"
                    : "staff";

            var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(uploadsRoot);

            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var fileName = $"{folder}_{userId}_{Guid.NewGuid():N}{extension}";
            var path = Path.Combine(uploadsRoot, fileName);

            await using var stream = System.IO.File.Create(path);
            await file.CopyToAsync(stream);

            return $"/uploads/{folder}/{fileName}";
        }

        private async Task<string?> SavePatientPhotoAsync(IFormFile file, int patientId)
        {
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(extension)) return null;

            var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", "patients");
            Directory.CreateDirectory(uploadsRoot);
            var fileName = $"patient_{patientId}_{Guid.NewGuid():N}{extension}";
            var path = Path.Combine(uploadsRoot, fileName);
            await using var stream = System.IO.File.Create(path);
            await file.CopyToAsync(stream);
            return $"/uploads/patients/{fileName}";
        }
    }
}
