using Hms.Web.Models.Auth;
using Hms.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hms.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthGatewayService _authGatewayService;

        public AccountController(AuthGatewayService authGatewayService)
        {
            _authGatewayService = authGatewayService;
        }

        [HttpGet]
        public IActionResult ActivatePortal()
        {
            return View(new PortalActivationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivatePortal(PortalActivationViewModel model)
        {
            if (model.PatientId <= 0 || string.IsNullOrWhiteSpace(model.MobileNumber))
            {
                ModelState.AddModelError(string.Empty, "Patient ID and Mobile Number are required.");
                return View(model);
            }

            var result = await _authGatewayService.SendPortalActivationAsync(model.PatientId, model.MobileNumber);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["Success"] = "Activation OTP sent successfully. Check the Auth API console output.";

            return RedirectToAction(nameof(VerifyPortalOtp), new
            {
                patientId = model.PatientId,
                mobileNumber = model.MobileNumber
            });
        }

        [HttpGet]
        public IActionResult VerifyPortalOtp(int patientId, string mobileNumber)
        {
            var model = new PortalActivationViewModel
            {
                PatientId = patientId,
                MobileNumber = mobileNumber
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPortalOtp(PortalActivationViewModel model)
        {
            if (model.PatientId <= 0 ||
                string.IsNullOrWhiteSpace(model.MobileNumber) ||
                string.IsNullOrWhiteSpace(model.OtpCode))
            {
                ModelState.AddModelError(string.Empty, "Patient ID, Mobile Number, and OTP are required.");
                return View(model);
            }

            var request = new VerifyOtpRequestDto
            {
                PatientId = model.PatientId,
                MobileNumber = model.MobileNumber,
                OtpCode = model.OtpCode,
                Purpose = "PortalActivation"
            };

            var result = await _authGatewayService.VerifyPortalOtpAsync(request);

            if (!result.Success || result.Data == null)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            HttpContext.Session.SetString("AccessToken", result.Data.AccessToken);
            HttpContext.Session.SetString("Role", result.Data.Role);
            HttpContext.Session.SetString("MobileNumber", result.Data.MobileNumber);
            HttpContext.Session.SetString("UHID", result.Data.UHID);
            HttpContext.Session.SetInt32("PatientId", result.Data.PatientId);
            HttpContext.Session.SetInt32("UserId", result.Data.UserId);

            TempData["Success"] = "Portal activated successfully.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new PatientLoginViewModel());
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

                var result = await _authGatewayService.SendLoginOtpAsync(model.PatientId, model.MobileNumber); if (!result.Success)
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

                HttpContext.Session.SetString("AccessToken", result.Data.AccessToken);
                HttpContext.Session.SetString("Role", result.Data.Role);
                HttpContext.Session.SetString("MobileNumber", result.Data.MobileNumber);
                HttpContext.Session.SetString("UHID", result.Data.UHID);
                HttpContext.Session.SetInt32("PatientId", result.Data.PatientId);
                HttpContext.Session.SetInt32("UserId", result.Data.UserId);

                TempData["Success"] = "Login successful.";
                return RedirectToAction(nameof(Profile));
            }

            ModelState.AddModelError(string.Empty, "Invalid action.");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var token = HttpContext.Session.GetString("AccessToken");

            if (string.IsNullOrWhiteSpace(token))
                return RedirectToAction(nameof(Login));

            var user = await _authGatewayService.GetCurrentUserAsync(token);

            if (user == null)
            {
                TempData["Error"] = "Session expired or unauthorized.";
                return RedirectToAction(nameof(Login));
            }

            return View(user);
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "Logged out successfully.";
            return RedirectToAction(nameof(Login));
        }
    }
}