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
        public IActionResult Index()
        {
            return RedirectToAction(nameof(ByPatient));
        }
        [HttpGet]
        [RequireRole("Admin", "Receptionist")]
        public async Task<IActionResult> Finance(int pageNumber = 1, int pageSize = 50)
        {
            var model = new FinanceDashboardViewModel();

            try
            {
                model.Summary = await _billingApiService.GetFinanceSummaryAsync() ?? new FinanceSummaryDto();
                model.Invoices = await _billingApiService.GetFinanceInvoicesAsync(pageNumber, pageSize);
            }
            catch (ApiException ex)
            {
                model.ErrorMessage = ex.Message;
            }
            catch (HttpRequestException ex)
            {
                model.ErrorMessage =
                    $"Could not connect to API Gateway. Check that the Gateway is running on the URL configured in ApiSettings:BaseUrl. Details: {ex.Message}";
            }
            catch (Exception ex)
            {
                model.ErrorMessage =
                    $"Something went wrong while loading Finance data. Details: {ex.Message}";
            }

            return View(model);
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
                return RedirectToAction(nameof(Details), new { invoiceId = result.Id });
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(request);
            }
        }

        [HttpGet("/Billing/{invoiceId:int}")]
        [HttpGet("/Billing/Details")]
        [HttpGet("/Billing/Details/{id:int}")]
        public async Task<IActionResult> Details(int invoiceId, int? id)
        {
            invoiceId = invoiceId > 0 ? invoiceId : (id ?? 0);

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

        [HttpGet("/Billing/Appointment/{appointmentId:int}")]
        [HttpGet("/Billing/DetailsByAppointment/{appointmentId:int}")]
        public async Task<IActionResult> DetailsByAppointment(int appointmentId)
        {
            if (appointmentId <= 0)
            {
                TempData["Error"] = "Invalid appointment.";
                return RedirectToAction(nameof(ByPatient));
            }

            try
            {
                var result = await _billingApiService.GetInvoiceByAppointmentIdAsync(appointmentId);

                if (result == null)
                {
                    TempData["Error"] = "Bill is not available yet. If you just completed the appointment, wait a few seconds and refresh because billing is generated in the background.";
                    return RedirectToAction(nameof(ByPatient));
                }

                return RedirectToAction(nameof(Details), new { invoiceId = result.Id });
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(ByPatient));
            }
        }

        [HttpGet]
        public async Task<IActionResult> ByPatient(int? patientId, bool showRecent = false)
        {
            var role = HttpContext.Session.GetString("Role");
            var isPatient = string.Equals(role, "Patient", StringComparison.OrdinalIgnoreCase);

            var hasPatientSearch = Request.Query.ContainsKey("patientId");
            var effectivePatientId = 0;

            if (isPatient)
            {
                effectivePatientId = HttpContext.Session.GetInt32("PatientId") ?? 0;
                hasPatientSearch = true;
            }
            else if (hasPatientSearch && patientId.HasValue)
            {
                effectivePatientId = patientId.Value;
            }

            ViewBag.PatientId = effectivePatientId > 0 ? (int?)effectivePatientId : null;
            ViewBag.ShowingRecentInvoices = false;

            try
            {
                if (effectivePatientId > 0)
                {
                    var result = await _billingApiService.GetInvoicesByPatientIdAsync(effectivePatientId);
                    return View(result ?? new List<InvoiceResponseDto>());
                }

                if (!isPatient && showRecent)
                {
                    var recent = await _billingApiService.GetFinanceInvoicesAsync(1, 50);
                    ViewBag.ShowingRecentInvoices = true;
                    return View(recent.Items ?? new List<InvoiceResponseDto>());
                }

                return View(new List<InvoiceResponseDto>());
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
                return RedirectToAction(nameof(Details), new { invoiceId = result.Id });
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
                return RedirectToAction(nameof(Details), new { invoiceId });
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
                return RedirectToAction(nameof(Details), new { invoiceId = result.Id });
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { invoiceId });
            }
        }
    }
}