using Frontend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers
{
    public class ProfileController : Controller
    {
        private readonly AuthGatewayService _authGatewayService;

        public ProfileController(AuthGatewayService authGatewayService)
        {
            _authGatewayService = authGatewayService;
        }

        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("AccessToken");
            if (string.IsNullOrWhiteSpace(token))
                return RedirectToAction("StaffLogin", "Account");

            var profile = await _authGatewayService.GetCurrentUserAsync(token);
            if (profile == null)
            {
                TempData["Error"] = "Session expired or unauthorized. Please login again.";
                return RedirectToAction("StaffLogin", "Account");
            }

            return View(profile);
        }
    }
}
