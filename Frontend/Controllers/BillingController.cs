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

            if (!ModelState.IsValid)
                return View(request);

            try
            {
                var result = await _billingApiService.CreateInvoiceAsync(request);

                if (result == null)
                {
                    ModelState.AddModelError(string.Empty, "Invoice could not be created.");
                    return View(request);
                }

                TempData["Success"] = "Invoice created successfully.";

                return RedirectToAction(nameof(Details), new
                {
                    invoiceId = result.Id
                });
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
            if (invoiceId <= 0)
            {
                TempData["Error"] = "Invalid invoice.";
                return RedirectToAction(nameof(ByPatient));
            }

            try
            {
                var result = await _billingApiService.GetInvoiceByIdAsync(invoiceId);

                if (result == null)
                {
                    TempData["Error"] = "Invoice not found.";
                    return RedirectToAction(nameof(ByPatient));
                }

                var role = HttpContext.Session.GetString("Role");
                if (string.Equals(role, "Patient", StringComparison.OrdinalIgnoreCase))
                {
                    var sessionPatientId = HttpContext.Session.GetInt32("PatientId") ?? 0;
                    if (sessionPatientId <= 0 || result.PatientId != sessionPatientId)
                    {
                        TempData["Error"] = "You can view only your own billing details.";
                        return RedirectToAction(nameof(ByPatient));
                    }
                }

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
            {
                effectivePatientId = HttpContext.Session.GetInt32("PatientId") ?? 0;
            }

            if (effectivePatientId <= 0)
            {
                ViewBag.PatientId = null;
                return View(new List<InvoiceResponseDto>());
            }

            try
            {
                ViewBag.PatientId = effectivePatientId;

                var result = await _billingApiService.GetInvoicesByPatientIdAsync(effectivePatientId);

                return View(result ?? new List<InvoiceResponseDto>());
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
            if (invoiceId <= 0)
            {
                TempData["Error"] = "Invalid invoice.";
                return RedirectToAction(nameof(ByPatient));
            }

            ViewBag.InvoiceId = invoiceId;
            return View(new AddInvoiceItemRequestDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole("Admin", "Receptionist")]
        public async Task<IActionResult> AddItem(int invoiceId, AddInvoiceItemRequestDto request)
        {
            ViewBag.InvoiceId = invoiceId;

            if (invoiceId <= 0)
            {
                ModelState.AddModelError(string.Empty, "Invalid invoice.");
                return View(request);
            }

            if (!ModelState.IsValid)
                return View(request);

            try
            {
                var result = await _billingApiService.AddInvoiceItemAsync(invoiceId, request);

                if (result == null)
                {
                    TempData["Error"] = "Invoice not found.";
                    return RedirectToAction(nameof(ByPatient));
                }

                TempData["Success"] = "Invoice item added successfully.";

                return RedirectToAction(nameof(Details), new
                {
                    invoiceId = result.Id
                });
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
            if (invoiceId <= 0)
            {
                TempData["Error"] = "Invalid invoice.";
                return RedirectToAction(nameof(ByPatient));
            }

            ViewBag.InvoiceId = invoiceId;

            return View(new PaymentRequestDto
            {
                PaymentMode = "Cash",
                PaymentDateUtc = DateTime.UtcNow
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole("Admin", "Receptionist")]
        public async Task<IActionResult> AddPayment(int invoiceId, PaymentRequestDto request)
        {
            ViewBag.InvoiceId = invoiceId;

            if (invoiceId <= 0)
            {
                ModelState.AddModelError(string.Empty, "Invalid invoice.");
                return View(request);
            }

            if (request.Amount <= 0)
            {
                ModelState.AddModelError(nameof(request.Amount), "Payment amount must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(request.PaymentMode))
            {
                request.PaymentMode = "Cash";
            }

            request.PaymentDateUtc = request.PaymentDateUtc == default
                ? DateTime.UtcNow
                : request.PaymentDateUtc;

            if (!ModelState.IsValid)
                return View(request);

            try
            {
                var result = await _billingApiService.AddPaymentAsync(invoiceId, request);

                if (result == null)
                {
                    TempData["Error"] = "Invoice not found.";
                    return RedirectToAction(nameof(ByPatient));
                }

                TempData["Success"] = "Payment received successfully.";

                return RedirectToAction(nameof(Details), new
                {
                    invoiceId = result.Id
                });
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(request);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole("Admin", "Receptionist")]
        public async Task<IActionResult> ReceivePayment(
            int invoiceId,
            decimal amount,
            string? paymentMode,
            string? referenceNumber,
            string? notes)
        {
            if (invoiceId <= 0)
            {
                TempData["Error"] = "Invalid invoice.";
                return RedirectToAction(nameof(ByPatient));
            }

            if (amount <= 0)
            {
                TempData["Error"] = "Payment amount must be greater than zero.";

                return RedirectToAction(nameof(Details), new
                {
                    invoiceId
                });
            }

            try
            {
                var request = new PaymentRequestDto
                {
                    Amount = amount,
                    PaymentMode = string.IsNullOrWhiteSpace(paymentMode) ? "Cash" : paymentMode,
                    ReferenceNumber = referenceNumber,
                    Notes = notes,
                    PaymentDateUtc = DateTime.UtcNow
                };

                var result = await _billingApiService.AddPaymentAsync(invoiceId, request);

                if (result == null)
                {
                    TempData["Error"] = "Invoice not found.";
                    return RedirectToAction(nameof(ByPatient));
                }

                TempData["Success"] = "Payment received successfully.";

                return RedirectToAction(nameof(Details), new
                {
                    invoiceId = result.Id
                });
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;

                return RedirectToAction(nameof(Details), new
                {
                    invoiceId
                });
            }
        }
    }
}