using Frontend.Infrastructure;
using Frontend.Models.Billing;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers
{
    [RequireRole("Admin", "Receptionist", "Patient")]
    public class BillingController : Controller
    {
        private readonly IBillingApiService _billingApiService;

        public BillingController(IBillingApiService billingApiService)
        {
            _billingApiService = billingApiService;
        }

        [HttpGet]
        [RequireRole("Admin", "Receptionist")]
        public IActionResult CreateInvoice()
        {
            return View(new CreateInvoiceRequestDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole("Admin", "Receptionist")]
        public async Task<IActionResult> CreateInvoice(CreateInvoiceRequestDto request)
        {
            request.Items = request.Items
                .Where(x => !string.IsNullOrWhiteSpace(x.ServiceName) && x.Price > 0 && x.Quantity > 0)
                .ToList();

            if (!ModelState.IsValid) return View(request);
            try
            {
                var result = await _billingApiService.CreateInvoiceAsync(request);
                if (result == null)
                {
                    ModelState.AddModelError(string.Empty, "Invoice could not be created.");
                    return View(request);
                }
                TempData["Success"] = "Invoice created successfully.";
                return RedirectToAction(nameof(Details), new { invoiceId = result.Id });
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(request);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int invoiceId)
        {
            try
            {
                var result = await _billingApiService.GetInvoiceByIdAsync(invoiceId);
                if (result == null) return NotFound();
                return View(result);
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(ByPatient));
            }
        }

        [HttpGet]
        public async Task<IActionResult> ByPatient(int? patientId)
        {
            var role = HttpContext.Session.GetString("Role");
            var effectivePatientId = patientId ?? 0;

            if (string.Equals(role, "Patient", StringComparison.OrdinalIgnoreCase))
                effectivePatientId = HttpContext.Session.GetInt32("PatientId") ?? 0;

            if (effectivePatientId <= 0)
            {
                ViewBag.PatientId = null;
                return View(new List<InvoiceResponseDto>());
            }

            try
            {
                ViewBag.PatientId = effectivePatientId;
                var result = await _billingApiService.GetInvoicesByPatientIdAsync(effectivePatientId);
                return View(result);
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(new List<InvoiceResponseDto>());
            }
        }

        [HttpGet]
        [RequireRole("Admin", "Receptionist")]
        public IActionResult AddItem(int invoiceId)
        {
            ViewBag.InvoiceId = invoiceId;
            return View(new AddInvoiceItemRequestDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole("Admin", "Receptionist")]
        public async Task<IActionResult> AddItem(int invoiceId, AddInvoiceItemRequestDto request)
        {
            ViewBag.InvoiceId = invoiceId;
            if (!ModelState.IsValid) return View(request);
            try
            {
                var result = await _billingApiService.AddInvoiceItemAsync(invoiceId, request);
                if (result == null) return NotFound();
                TempData["Success"] = "Invoice item added successfully.";
                return RedirectToAction(nameof(Details), new { invoiceId = result.Id });
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(request);
            }
        }

        [HttpGet]
        [RequireRole("Admin", "Receptionist")]
        public IActionResult AddPayment(int invoiceId)
        {
            ViewBag.InvoiceId = invoiceId;
            return View(new PaymentRequestDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole("Admin", "Receptionist")]
        public async Task<IActionResult> AddPayment(int invoiceId, PaymentRequestDto request)
        {
            ViewBag.InvoiceId = invoiceId;
            if (!ModelState.IsValid) return View(request);
            try
            {
                var result = await _billingApiService.AddPaymentAsync(invoiceId, request);
                if (result == null) return NotFound();
                TempData["Success"] = "Payment added successfully.";
                return RedirectToAction(nameof(Details), new { invoiceId = result.Id });
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(request);
            }
        }
    }
}
