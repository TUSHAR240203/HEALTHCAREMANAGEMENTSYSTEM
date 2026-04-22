using Frontend.Models.Billing;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers
{
    public class BillingController : Controller
    {
        private readonly IBillingApiService _billingApiService;

        public BillingController(IBillingApiService billingApiService)
        {
            _billingApiService = billingApiService;
        }

        [HttpGet]
        public IActionResult CreateInvoice()
        {
            return View(new CreateInvoiceRequestDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateInvoice(CreateInvoiceRequestDto request)
        {
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
                return RedirectToAction(nameof(CreateInvoice));
            }
        }

        [HttpGet]
        public IActionResult ByPatient()
        {
            return View(new List<InvoiceResponseDto>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ByPatient(int patientId)
        {
            try
            {
                ViewBag.PatientId = patientId;
                var result = await _billingApiService.GetInvoicesByPatientIdAsync(patientId);
                return View(result);
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(new List<InvoiceResponseDto>());
            }
        }

        [HttpGet]
        public IActionResult AddItem(int invoiceId)
        {
            ViewBag.InvoiceId = invoiceId;
            return View(new AddInvoiceItemRequestDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(int invoiceId, AddInvoiceItemRequestDto request)
        {
            ViewBag.InvoiceId = invoiceId;

            if (!ModelState.IsValid)
                return View(request);

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
        public IActionResult AddPayment(int invoiceId)
        {
            ViewBag.InvoiceId = invoiceId;
            return View(new PaymentRequestDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPayment(int invoiceId, PaymentRequestDto request)
        {
            ViewBag.InvoiceId = invoiceId;

            if (!ModelState.IsValid)
                return View(request);

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
