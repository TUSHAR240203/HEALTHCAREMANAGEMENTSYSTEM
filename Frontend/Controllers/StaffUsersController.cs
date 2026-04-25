using Frontend.Infrastructure;
using Frontend.Models.Admin;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers
{
    [RequireRole("Admin")]
    public class StaffUsersController : Controller
    {
        private readonly StaffUserGatewayService _staffUserGatewayService;

        public StaffUsersController(StaffUserGatewayService staffUserGatewayService)
        {
            _staffUserGatewayService = staffUserGatewayService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("AccessToken") ?? string.Empty;
            var users = await _staffUserGatewayService.GetUsersAsync(token);
            return View(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateStaffUserViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStaffUserViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var token = HttpContext.Session.GetString("AccessToken") ?? string.Empty;
            var result = await _staffUserGatewayService.CreateAsync(token, model);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["Success"] = "Login account created. Create the matching doctor profile from Doctors if this is a doctor.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id, bool isActive)
        {
            var token = HttpContext.Session.GetString("AccessToken") ?? string.Empty;
            var result = await _staffUserGatewayService.UpdateStatusAsync(token, id, isActive);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var token = HttpContext.Session.GetString("AccessToken") ?? string.Empty;
            var result = await _staffUserGatewayService.DeleteAsync(token, id);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
