$ErrorActionPreference = "Stop"

$frontend = Join-Path $PSScriptRoot "Frontend"

if (-not (Test-Path $frontend)) {
    throw "Frontend folder not found. Put this script in the solution root."
}

function Ensure-Dir($path) {
    if (-not (Test-Path $path)) {
        New-Item -ItemType Directory -Path $path -Force | Out-Null
    }
}

function Write-Utf8File($path, $content) {
    $dir = Split-Path $path -Parent
    Ensure-Dir $dir
    Set-Content -Path $path -Value $content -Encoding UTF8
    Write-Host "Written: $path"
}

$modelsDir = Join-Path $frontend "Models\Billing"
$servicesDir = Join-Path $frontend "Services"
$controllersDir = Join-Path $frontend "Controllers"
$viewsDir = Join-Path $frontend "Views\Billing"

Ensure-Dir $modelsDir
Ensure-Dir $servicesDir
Ensure-Dir $controllersDir
Ensure-Dir $viewsDir

# -----------------------------
# Models
# -----------------------------

Write-Utf8File (Join-Path $modelsDir "CreateInvoiceRequestDto.cs") @'
namespace Frontend.Models.Billing
{
    public class CreateInvoiceRequestDto
    {
        public int PatientId { get; set; }
        public string UHID { get; set; } = string.Empty;
        public int AppointmentId { get; set; }
        public decimal ConsultationFee { get; set; }
    }
}
'@

Write-Utf8File (Join-Path $modelsDir "AddInvoiceItemRequestDto.cs") @'
namespace Frontend.Models.Billing
{
    public class AddInvoiceItemRequestDto
    {
        public string ServiceName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
'@

Write-Utf8File (Join-Path $modelsDir "PaymentRequestDto.cs") @'
namespace Frontend.Models.Billing
{
    public class PaymentRequestDto
    {
        public decimal Amount { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
    }
}
'@

Write-Utf8File (Join-Path $modelsDir "InvoiceItemResponseDto.cs") @'
namespace Frontend.Models.Billing
{
    public class InvoiceItemResponseDto
    {
        public int Id { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
'@

Write-Utf8File (Join-Path $modelsDir "PaymentResponseDto.cs") @'
namespace Frontend.Models.Billing
{
    public class PaymentResponseDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
        public DateTime PaidAtUtc { get; set; }
    }
}
'@

Write-Utf8File (Join-Path $modelsDir "InvoiceResponseDto.cs") @'
using System.Collections.Generic;

namespace Frontend.Models.Billing
{
    public class InvoiceResponseDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string UHID { get; set; } = string.Empty;
        public int AppointmentId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public List<InvoiceItemResponseDto> Items { get; set; } = new();
        public List<PaymentResponseDto> Payments { get; set; } = new();
    }
}
'@

# -----------------------------
# Services
# -----------------------------

Write-Utf8File (Join-Path $servicesDir "IBillingApiService.cs") @'
using Frontend.Models.Billing;

namespace Frontend.Services
{
    public interface IBillingApiService
    {
        Task<InvoiceResponseDto?> CreateInvoiceAsync(CreateInvoiceRequestDto request);
        Task<InvoiceResponseDto?> GetInvoiceByIdAsync(int invoiceId);
        Task<List<InvoiceResponseDto>> GetInvoicesByPatientIdAsync(int patientId);
        Task<InvoiceResponseDto?> AddInvoiceItemAsync(int invoiceId, AddInvoiceItemRequestDto request);
        Task<InvoiceResponseDto?> AddPaymentAsync(int invoiceId, PaymentRequestDto request);
    }
}
'@

Write-Utf8File (Join-Path $servicesDir "BillingApiService.cs") @'
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Frontend.Models.Billing;

namespace Frontend.Services
{
    public class BillingApiService : IBillingApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public BillingApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        public async Task<InvoiceResponseDto?> CreateInvoiceAsync(CreateInvoiceRequestDto request)
        {
            return await PostAsync<CreateInvoiceRequestDto, InvoiceResponseDto>(
                "gateway/billing/invoice", request);
        }

        public async Task<InvoiceResponseDto?> GetInvoiceByIdAsync(int invoiceId)
        {
            return await GetAsync<InvoiceResponseDto>($"gateway/billing/invoice/{invoiceId}", true);
        }

        public async Task<List<InvoiceResponseDto>> GetInvoicesByPatientIdAsync(int patientId)
        {
            return await GetAsync<List<InvoiceResponseDto>>($"gateway/billing/patient/{patientId}/invoices")
                   ?? new List<InvoiceResponseDto>();
        }

        public async Task<InvoiceResponseDto?> AddInvoiceItemAsync(int invoiceId, AddInvoiceItemRequestDto request)
        {
            return await PostAsync<AddInvoiceItemRequestDto, InvoiceResponseDto>(
                $"gateway/billing/invoice/{invoiceId}/items", request);
        }

        public async Task<InvoiceResponseDto?> AddPaymentAsync(int invoiceId, PaymentRequestDto request)
        {
            return await PostAsync<PaymentRequestDto, InvoiceResponseDto>(
                $"gateway/billing/invoice/{invoiceId}/pay", request);
        }

        private async Task<TResponse?> GetAsync<TResponse>(string url, bool allowNotFound = false)
        {
            using var response = await _httpClient.GetAsync(url);
            return await ReadResponseAsync<TResponse>(response, allowNotFound);
        }

        private async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest request)
        {
            using var content = CreateJsonContent(request);
            using var response = await _httpClient.PostAsync(url, content);
            return await ReadResponseAsync<TResponse>(response, false);
        }

        private StringContent CreateJsonContent<TRequest>(TRequest request)
        {
            return new StringContent(
                JsonSerializer.Serialize(request, _jsonOptions),
                Encoding.UTF8,
                "application/json");
        }

        private async Task<TResponse?> ReadResponseAsync<TResponse>(HttpResponseMessage response, bool allowNotFound)
        {
            if (allowNotFound && response.StatusCode == HttpStatusCode.NotFound)
                return default;

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(
                    string.IsNullOrWhiteSpace(body)
                        ? $"API request failed with status code {(int)response.StatusCode}."
                        : body,
                    (int)response.StatusCode);
            }

            if (string.IsNullOrWhiteSpace(body))
                return default;

            return JsonSerializer.Deserialize<TResponse>(body, _jsonOptions);
        }
    }
}
'@

# -----------------------------
# Controller
# -----------------------------

Write-Utf8File (Join-Path $controllersDir "BillingController.cs") @'
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
'@

# -----------------------------
# Views
# -----------------------------

Write-Utf8File (Join-Path $viewsDir "CreateInvoice.cshtml") @'
@model Frontend.Models.Billing.CreateInvoiceRequestDto

<h2>Create Invoice</h2>

<form asp-action="CreateInvoice" method="post">
    @Html.AntiForgeryToken()

    <div class="mb-3">
        <label asp-for="PatientId" class="form-label"></label>
        <input asp-for="PatientId" class="form-control" />
    </div>

    <div class="mb-3">
        <label asp-for="UHID" class="form-label"></label>
        <input asp-for="UHID" class="form-control" />
    </div>

    <div class="mb-3">
        <label asp-for="AppointmentId" class="form-label"></label>
        <input asp-for="AppointmentId" class="form-control" />
    </div>

    <div class="mb-3">
        <label asp-for="ConsultationFee" class="form-label"></label>
        <input asp-for="ConsultationFee" class="form-control" />
    </div>

    <button type="submit" class="btn btn-primary">Create Invoice</button>
</form>
'@

Write-Utf8File (Join-Path $viewsDir "Details.cshtml") @'
@model Frontend.Models.Billing.InvoiceResponseDto

<h2>Invoice Details</h2>

<table class="table table-bordered">
    <tr><th>Invoice Id</th><td>@Model.Id</td></tr>
    <tr><th>Patient Id</th><td>@Model.PatientId</td></tr>
    <tr><th>UHID</th><td>@Model.UHID</td></tr>
    <tr><th>Appointment Id</th><td>@Model.AppointmentId</td></tr>
    <tr><th>Total Amount</th><td>@Model.TotalAmount</td></tr>
    <tr><th>Paid Amount</th><td>@Model.PaidAmount</td></tr>
    <tr><th>Balance Amount</th><td>@Model.BalanceAmount</td></tr>
    <tr><th>Status</th><td>@Model.Status</td></tr>
    <tr><th>Created At (UTC)</th><td>@Model.CreatedAtUtc</td></tr>
</table>

<p>
    <a asp-action="AddItem" asp-route-invoiceId="@Model.Id" class="btn btn-sm btn-primary">Add Item</a>
    <a asp-action="AddPayment" asp-route-invoiceId="@Model.Id" class="btn btn-sm btn-success">Add Payment</a>
</p>

<h4>Items</h4>
@if (Model.Items != null && Model.Items.Any())
{
    <table class="table table-striped">
        <thead>
            <tr>
                <th>Id</th>
                <th>Service Name</th>
                <th>Amount</th>
            </tr>
        </thead>
        <tbody>
        @foreach (var item in Model.Items)
        {
            <tr>
                <td>@item.Id</td>
                <td>@item.ServiceName</td>
                <td>@item.Amount</td>
            </tr>
        }
        </tbody>
    </table>
}
else
{
    <div class="alert alert-warning">No invoice items found.</div>
}

<h4>Payments</h4>
@if (Model.Payments != null && Model.Payments.Any())
{
    <table class="table table-striped">
        <thead>
            <tr>
                <th>Id</th>
                <th>Amount</th>
                <th>Payment Mode</th>
                <th>Paid At (UTC)</th>
            </tr>
        </thead>
        <tbody>
        @foreach (var payment in Model.Payments)
        {
            <tr>
                <td>@payment.Id</td>
                <td>@payment.Amount</td>
                <td>@payment.PaymentMode</td>
                <td>@payment.PaidAtUtc</td>
            </tr>
        }
        </tbody>
    </table>
}
else
{
    <div class="alert alert-warning">No payments found.</div>
}
'@

Write-Utf8File (Join-Path $viewsDir "ByPatient.cshtml") @'
@model List<Frontend.Models.Billing.InvoiceResponseDto>

<h2>Invoices By Patient</h2>

<form asp-action="ByPatient" method="post">
    @Html.AntiForgeryToken()

    <div class="mb-3">
        <label class="form-label">Patient Id</label>
        <input name="patientId" value="@ViewBag.PatientId" class="form-control" />
    </div>

    <button type="submit" class="btn btn-primary">Search</button>
</form>

<hr />

@if (Model != null && Model.Any())
{
    <table class="table table-bordered">
        <thead>
            <tr>
                <th>Invoice Id</th>
                <th>UHID</th>
                <th>Appointment Id</th>
                <th>Total</th>
                <th>Paid</th>
                <th>Balance</th>
                <th>Status</th>
                <th>Action</th>
            </tr>
        </thead>
        <tbody>
        @foreach (var invoice in Model)
        {
            <tr>
                <td>@invoice.Id</td>
                <td>@invoice.UHID</td>
                <td>@invoice.AppointmentId</td>
                <td>@invoice.TotalAmount</td>
                <td>@invoice.PaidAmount</td>
                <td>@invoice.BalanceAmount</td>
                <td>@invoice.Status</td>
                <td>
                    <a asp-action="Details" asp-route-invoiceId="@invoice.Id" class="btn btn-sm btn-info">View</a>
                </td>
            </tr>
        }
        </tbody>
    </table>
}
'@

Write-Utf8File (Join-Path $viewsDir "AddItem.cshtml") @'
@model Frontend.Models.Billing.AddInvoiceItemRequestDto

<h2>Add Invoice Item</h2>

<form asp-action="AddItem" asp-route-invoiceId="@ViewBag.InvoiceId" method="post">
    @Html.AntiForgeryToken()

    <div class="mb-3">
        <label asp-for="ServiceName" class="form-label"></label>
        <input asp-for="ServiceName" class="form-control" />
    </div>

    <div class="mb-3">
        <label asp-for="Amount" class="form-label"></label>
        <input asp-for="Amount" class="form-control" />
    </div>

    <button type="submit" class="btn btn-primary">Add Item</button>
</form>
'@

Write-Utf8File (Join-Path $viewsDir "AddPayment.cshtml") @'
@model Frontend.Models.Billing.PaymentRequestDto

<h2>Add Payment</h2>

<form asp-action="AddPayment" asp-route-invoiceId="@ViewBag.InvoiceId" method="post">
    @Html.AntiForgeryToken()

    <div class="mb-3">
        <label asp-for="Amount" class="form-label"></label>
        <input asp-for="Amount" class="form-control" />
    </div>

    <div class="mb-3">
        <label asp-for="PaymentMode" class="form-label"></label>
        <input asp-for="PaymentMode" class="form-control" />
    </div>

    <button type="submit" class="btn btn-success">Add Payment</button>
</form>
'@

# -----------------------------
# Program.cs patch
# -----------------------------

$programPath = Join-Path $frontend "Program.cs"
if (Test-Path $programPath) {
    $programText = Get-Content $programPath -Raw

    if ($programText -notmatch 'AddHttpClient<IBillingApiService,\s*BillingApiService>') {
        $registration = @'

builder.Services.AddHttpClient<IBillingApiService, BillingApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7000/");
});
'@

        if ($programText -match 'builder\.Services\.AddHttpClient<IReceptionApiService,\s*ReceptionApiService>\(client =>\s*\{.*?\}\);\s*') {
            $programText = [regex]::Replace(
                $programText,
                '(builder\.Services\.AddHttpClient<IReceptionApiService,\s*ReceptionApiService>\(client =>\s*\{.*?\}\);\s*)',
                "`$1$registration",
                [System.Text.RegularExpressions.RegexOptions]::Singleline
            )
        }
        elseif ($programText -match 'builder\.Services\.AddHttpClient<IAppointmentApiService,\s*AppointmentApiService>\(client =>\s*\{.*?\}\);\s*') {
            $programText = [regex]::Replace(
                $programText,
                '(builder\.Services\.AddHttpClient<IAppointmentApiService,\s*AppointmentApiService>\(client =>\s*\{.*?\}\);\s*)',
                "`$1$registration",
                [System.Text.RegularExpressions.RegexOptions]::Singleline
            )
        }
        else {
            $programText += $registration
        }

        Set-Content -Path $programPath -Value $programText -Encoding UTF8
        Write-Host "Updated: $programPath"
    }
    else {
        Write-Host "Skipped Program.cs update: billing HttpClient already exists."
    }
}
else {
    Write-Warning "Program.cs not found, skipping registration."
}

# -----------------------------
# _Layout.cshtml patch
# -----------------------------

$layoutPath = Join-Path $frontend "Views\Shared\_Layout.cshtml"
if (Test-Path $layoutPath) {
    $layoutText = Get-Content $layoutPath -Raw

    if ($layoutText -notmatch 'asp-controller="Billing"') {
        $navBlock = @'

<li class="nav-item">
    <a class="nav-link text-dark" asp-controller="Billing" asp-action="CreateInvoice">Create Invoice</a>
</li>
<li class="nav-item">
    <a class="nav-link text-dark" asp-controller="Billing" asp-action="ByPatient">Billing By Patient</a>
</li>
'@

        if ($layoutText -match '</ul>') {
            $layoutText = $layoutText -replace '</ul>', "$navBlock`r`n</ul>"
            Set-Content -Path $layoutPath -Value $layoutText -Encoding UTF8
            Write-Host "Updated: $layoutPath"
        }
        else {
            Write-Warning "_Layout.cshtml found, but </ul> not found. Add nav items manually."
        }
    }
    else {
        Write-Host "Skipped _Layout.cshtml update: billing nav already exists."
    }
}
else {
    Write-Warning "_Layout.cshtml not found, skipping nav update."
}

Write-Host ""
Write-Host "Billing frontend files added."
Write-Host "Now run:"
Write-Host "  dotnet clean"
Write-Host "  dotnet build"
Write-Host "Then start:"
Write-Host "  - HealthcareGateway"
Write-Host "  - Hms.BillingApi"
Write-Host "  - Frontend"