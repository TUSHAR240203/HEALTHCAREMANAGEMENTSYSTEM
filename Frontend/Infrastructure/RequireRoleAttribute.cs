using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Frontend.Infrastructure
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class RequireRoleAttribute : ActionFilterAttribute
    {
        private readonly HashSet<string> _roles;

        public RequireRoleAttribute(params string[] roles)
        {
            _roles = roles
                .Select(r => r.Trim())
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;

            var token = session.GetString("AccessToken");
            var role = session.GetString("Role") ?? string.Empty;

            var requiresPatient = _roles.Any(r =>
                string.Equals(r, "Patient", StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(role))
            {
                context.Result = new RedirectToActionResult(
                    requiresPatient ? "Login" : "StaffLogin",
                    "Account",
                    new { area = "" });

                return;
            }

            if (_roles.Count > 0 && !_roles.Contains(role))
            {
                if (context.Controller is Controller controller)
                {
                    controller.TempData["Error"] =
                        "You are not allowed to open that portal for your current role.";
                }

                if (string.Equals(role, "Patient", StringComparison.OrdinalIgnoreCase))
                {
                    context.Result = new RedirectToActionResult(
                        "Dashboard",
                        "PatientPortal",
                        new { area = "" });

                    return;
                }

                context.Result = new RedirectToActionResult(
                    "Index",
                    "Home",
                    new { area = "" });

                return;
            }

            base.OnActionExecuting(context);
        }
    }
}