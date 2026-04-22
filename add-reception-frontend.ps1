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

$modelsDir = Join-Path $frontend "Models\Reception"
$servicesDir = Join-Path $frontend "Services"
$controllersDir = Join-Path $frontend "Controllers"
$viewsDir = Join-Path $frontend "Views\Reception"

Ensure-Dir $modelsDir
Ensure-Dir $servicesDir
Ensure-Dir $controllersDir
Ensure-Dir $viewsDir

# -----------------------------
# Models
# -----------------------------

Write-Utf8File (Join-Path $modelsDir "ReceptionPatientSearchRequestDto.cs") @'
namespace Frontend.Models.Reception
{
    public class ReceptionPatientSearchRequestDto
    {
        public string? UHID { get; set; }
        public string? MobileNumber { get; set; }
        public string? Name { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
'@

Write-Utf8File (Join-Path $modelsDir "RegisterPatientByReceptionRequestDto.cs") @'
namespace Frontend.Models.Reception
{
    public class RegisterPatientByReceptionRequestDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public int Gender { get; set; }
        public string MobileNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? BloodGroup { get; set; }
        public string? AddressLine1 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactNumber { get; set; }
        public string? EmergencyContactRelation { get; set; }
        public string? InsuranceProvider { get; set; }
        public string? InsurancePolicyNumber { get; set; }
        public bool PortalAccessEnabled { get; set; }
        public bool SendPortalActivationSms { get; set; }
    }
}
'@

Write-Utf8File (Join-Path $modelsDir "VerifyPatientRequestDto.cs") @'
namespace Frontend.Models.Reception
{
    public class VerifyPatientRequestDto
    {
        public DateOnly? DateOfBirth { get; set; }
        public string? MobileNumber { get; set; }
    }
}
'@

Write-Utf8File (Join-Path $modelsDir "BookAppointmentRequestDto.cs") @'
namespace Frontend.Models.Reception
{
    public class BookAppointmentRequestDto
    {
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public int DepartmentId { get; set; }
        public DateOnly AppointmentDate { get; set; }
        public TimeOnly SlotStartTime { get; set; }
        public TimeOnly SlotEndTime { get; set; }
        public string VisitType { get; set; } = string.Empty;
        public string? ReasonForVisit { get; set; }
        public bool IsTeleConsultation { get; set; }
    }
}
'@

Write-Utf8File (Join-Path $modelsDir "CheckInRequestDto.cs") @'
namespace Frontend.Models.Reception
{
    public class CheckInRequestDto
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public int DepartmentId { get; set; }
        public DateTime CheckInTimeUtc { get; set; }
    }
}
'@

Write-Utf8File (Join-Path $modelsDir "CancelAppointmentRequestDto.cs") @'
namespace Frontend.Models.Reception
{
    public class CancelAppointmentRequestDto
    {
        public string? Reason { get; set; }
    }
}
'@

Write-Utf8File (Join-Path $modelsDir "RescheduleAppointmentRequestDto.cs") @'
namespace Frontend.Models.Reception
{
    public class RescheduleAppointmentRequestDto
    {
        public DateOnly NewAppointmentDate { get; set; }
        public TimeOnly NewSlotStartTime { get; set; }
        public TimeOnly NewSlotEndTime { get; set; }
        public string? Reason { get; set; }
    }
}
'@

Write-Utf8File (Join-Path $modelsDir "CompleteQueueTokenRequestDto.cs") @'
namespace Frontend.Models.Reception
{
    public class CompleteQueueTokenRequestDto
    {
        public string? Notes { get; set; }
    }
}
'@

Write-Utf8File (Join-Path $modelsDir "CancelQueueTokenRequestDto.cs") @'
namespace Frontend.Models.Reception
{
    public class CancelQueueTokenRequestDto
    {
        public string? Notes { get; set; }
    }
}
'@

Write-Utf8File (Join-Path $modelsDir "ReceptionPatientSummaryDto.cs") @'
namespace Frontend.Models.Reception
{
    public class ReceptionPatientSummaryDto
    {
        public int Id { get; set; }
        public string? UHID { get; set; }
        public string? FullName { get; set; }
        public string? MobileNumber { get; set; }
        public string? Email { get; set; }
        public DateOnly? DateOfBirth { get; set; }
    }
}
'@

Write-Utf8File (Join-Path $modelsDir "ReceptionPatientSearchResponseDto.cs") @'
using System.Collections.Generic;

namespace Frontend.Models.Reception
{
    public class ReceptionPatientSearchResponseDto
    {
        public List<ReceptionPatientSummaryDto> Patients { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
'@

Write-Utf8File (Join-Path $modelsDir "QueueItemDto.cs") @'
namespace Frontend.Models.Reception
{
    public class QueueItemDto
    {
        public int QueueTokenId { get; set; }
        public int TokenNumber { get; set; }
        public int PatientId { get; set; }
        public string UHID { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
'@

Write-Utf8File (Join-Path $modelsDir "DepartmentQueueResponseDto.cs") @'
using System.Collections.Generic;

namespace Frontend.Models.Reception
{
    public class DepartmentQueueResponseDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public List<QueueItemDto> Queue { get; set; } = new();
    }
}
'@

Write-Utf8File (Join-Path $modelsDir "QueueCurrentResponseDto.cs") @'
namespace Frontend.Models.Reception
{
    public class QueueCurrentResponseDto
    {
        public int QueueTokenId { get; set; }
        public int TokenNumber { get; set; }
        public int PatientId { get; set; }
        public string UHID { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? CalledAtUtc { get; set; }
        public DateTime? StartedAtUtc { get; set; }
    }
}
'@

# -----------------------------
# Services
# -----------------------------

Write-Utf8File (Join-Path $servicesDir "IReceptionApiService.cs") @'
using Frontend.Models.Reception;
using System.Threading.Tasks;

namespace Frontend.Services
{
    public interface IReceptionApiService
    {
        Task<ReceptionPatientSearchResponseDto?> SearchPatientsAsync(ReceptionPatientSearchRequestDto request);
        Task<ReceptionPatientSummaryDto?> GetPatientSummaryAsync(int patientId);
        Task<T?> RegisterPatientAsync<T>(RegisterPatientByReceptionRequestDto request);
        Task<T?> VerifyPatientAsync<T>(int patientId, VerifyPatientRequestDto request);
        Task<T?> BookAppointmentAsync<T>(BookAppointmentRequestDto request);
        Task<T?> CheckInAsync<T>(CheckInRequestDto request);
        Task<DepartmentQueueResponseDto?> GetQueueAsync(int departmentId, DateOnly date);
        Task<QueueCurrentResponseDto?> GetCurrentQueueAsync(int departmentId, DateOnly date);
        Task<T?> CallNextAsync<T>(int departmentId, DateOnly date);
        Task<T?> StartTokenAsync<T>(int queueTokenId);
        Task<T?> CompleteTokenAsync<T>(int queueTokenId, CompleteQueueTokenRequestDto request);
        Task<T?> SkipTokenAsync<T>(int queueTokenId);
        Task<T?> RecallTokenAsync<T>(int queueTokenId);
        Task<T?> CancelTokenAsync<T>(int queueTokenId, CancelQueueTokenRequestDto request);
    }
}
'@

Write-Utf8File (Join-Path $servicesDir "ReceptionApiService.cs") @'
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Frontend.Models.Reception;

namespace Frontend.Services
{
    public class ReceptionApiService : IReceptionApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public ReceptionApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        public async Task<ReceptionPatientSearchResponseDto?> SearchPatientsAsync(ReceptionPatientSearchRequestDto request)
            => await PostAsync<ReceptionPatientSearchRequestDto, ReceptionPatientSearchResponseDto>(
                "gateway/reception/patients/search", request);

        public async Task<ReceptionPatientSummaryDto?> GetPatientSummaryAsync(int patientId)
            => await GetAsync<ReceptionPatientSummaryDto>($"gateway/reception/patients/{patientId}/summary", true);

        public async Task<T?> RegisterPatientAsync<T>(RegisterPatientByReceptionRequestDto request)
            => await PostAsync<RegisterPatientByReceptionRequestDto, T>("gateway/reception/patients/register", request);

        public async Task<T?> VerifyPatientAsync<T>(int patientId, VerifyPatientRequestDto request)
            => await PostAsync<VerifyPatientRequestDto, T>($"gateway/reception/patients/{patientId}/verify", request);

        public async Task<T?> BookAppointmentAsync<T>(BookAppointmentRequestDto request)
            => await PostAsync<BookAppointmentRequestDto, T>("gateway/reception/appointments/book", request);

        public async Task<T?> CheckInAsync<T>(CheckInRequestDto request)
            => await PostAsync<CheckInRequestDto, T>("gateway/reception/checkin", request);

        public async Task<DepartmentQueueResponseDto?> GetQueueAsync(int departmentId, DateOnly date)
            => await GetAsync<DepartmentQueueResponseDto>($"gateway/reception/queue/{departmentId}?date={date:yyyy-MM-dd}");

        public async Task<QueueCurrentResponseDto?> GetCurrentQueueAsync(int departmentId, DateOnly date)
            => await GetAsync<QueueCurrentResponseDto>($"gateway/reception/queue/{departmentId}/current?date={date:yyyy-MM-dd}", true);

        public async Task<T?> CallNextAsync<T>(int departmentId, DateOnly date)
            => await PostAsync<object, T>($"gateway/reception/queue/{departmentId}/next?date={date:yyyy-MM-dd}", new { });

        public async Task<T?> StartTokenAsync<T>(int queueTokenId)
            => await PutAsync<object, T>($"gateway/reception/queue/token/{queueTokenId}/start", new { }, true);

        public async Task<T?> CompleteTokenAsync<T>(int queueTokenId, CompleteQueueTokenRequestDto request)
            => await PutAsync<CompleteQueueTokenRequestDto, T>($"gateway/reception/queue/token/{queueTokenId}/complete", request, true);

        public async Task<T?> SkipTokenAsync<T>(int queueTokenId)
            => await PutAsync<object, T>($"gateway/reception/queue/token/{queueTokenId}/skip", new { }, true);

        public async Task<T?> RecallTokenAsync<T>(int queueTokenId)
            => await PutAsync<object, T>($"gateway/reception/queue/token/{queueTokenId}/recall", new { }, true);

        public async Task<T?> CancelTokenAsync<T>(int queueTokenId, CancelQueueTokenRequestDto request)
            => await PutAsync<CancelQueueTokenRequestDto, T>($"gateway/reception/queue/token/{queueTokenId}/cancel", request, true);

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

        private async Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest request, bool allowNotFound)
        {
            using var content = CreateJsonContent(request);
            using var response = await _httpClient.PutAsync(url, content);
            return await ReadResponseAsync<TResponse>(response, allowNotFound);
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

Write-Utf8File (Join-Path $controllersDir "ReceptionController.cs") @'
using Frontend.Models.Reception;
using Frontend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers
{
    public class ReceptionController : Controller
    {
        private readonly IReceptionApiService _receptionApiService;

        public ReceptionController(IReceptionApiService receptionApiService)
        {
            _receptionApiService = receptionApiService;
        }

        [HttpGet]
        public IActionResult SearchPatients()
        {
            return View(new ReceptionPatientSearchRequestDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SearchPatients(ReceptionPatientSearchRequestDto request)
        {
            try
            {
                var result = await _receptionApiService.SearchPatientsAsync(request);
                ViewBag.Results = result?.Patients ?? new List<ReceptionPatientSummaryDto>();
                return View(request);
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(request);
            }
        }

        [HttpGet]
        public IActionResult RegisterPatient()
        {
            return View(new RegisterPatientByReceptionRequestDto
            {
                DateOfBirth = DateOnly.FromDateTime(DateTime.Today)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterPatient(RegisterPatientByReceptionRequestDto request)
        {
            if (!ModelState.IsValid)
                return View(request);

            try
            {
                await _receptionApiService.RegisterPatientAsync<object>(request);
                TempData["Success"] = "Patient registered successfully.";
                return RedirectToAction(nameof(SearchPatients));
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(request);
            }
        }

        [HttpGet]
        public async Task<IActionResult> PatientSummary(int patientId)
        {
            try
            {
                var result = await _receptionApiService.GetPatientSummaryAsync(patientId);
                if (result == null) return NotFound();
                return View(result);
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(SearchPatients));
            }
        }

        [HttpGet]
        public IActionResult BookAppointment()
        {
            return View(new BookAppointmentRequestDto
            {
                AppointmentDate = DateOnly.FromDateTime(DateTime.Today)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookAppointment(BookAppointmentRequestDto request)
        {
            if (!ModelState.IsValid)
                return View(request);

            try
            {
                await _receptionApiService.BookAppointmentAsync<object>(request);
                TempData["Success"] = "Appointment booked successfully.";
                return RedirectToAction(nameof(SearchPatients));
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(request);
            }
        }

        [HttpGet]
        public IActionResult CheckIn()
        {
            return View(new CheckInRequestDto
            {
                CheckInTimeUtc = DateTime.UtcNow
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(CheckInRequestDto request)
        {
            if (!ModelState.IsValid)
                return View(request);

            try
            {
                await _receptionApiService.CheckInAsync<object>(request);
                TempData["Success"] = "Patient checked in successfully.";
                return RedirectToAction(nameof(SearchPatients));
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(request);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Queue(int departmentId, DateOnly? date)
        {
            try
            {
                var queueDate = date ?? DateOnly.FromDateTime(DateTime.Today);
                var result = await _receptionApiService.GetQueueAsync(departmentId, queueDate);
                return View(result);
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(SearchPatients));
            }
        }
    }
}
'@

# -----------------------------
# Views
# -----------------------------

Write-Utf8File (Join-Path $viewsDir "SearchPatients.cshtml") @'
@model Frontend.Models.Reception.ReceptionPatientSearchRequestDto
@{
    var results = ViewBag.Results as List<Frontend.Models.Reception.ReceptionPatientSummaryDto>
                  ?? new List<Frontend.Models.Reception.ReceptionPatientSummaryDto>();
}

<h2>Search Patients</h2>

<form asp-action="SearchPatients" method="post">
    @Html.AntiForgeryToken()

    <div class="mb-3">
        <label asp-for="UHID" class="form-label"></label>
        <input asp-for="UHID" class="form-control" />
    </div>

    <div class="mb-3">
        <label asp-for="MobileNumber" class="form-label"></label>
        <input asp-for="MobileNumber" class="form-control" />
    </div>

    <div class="mb-3">
        <label asp-for="Name" class="form-label"></label>
        <input asp-for="Name" class="form-control" />
    </div>

    <button type="submit" class="btn btn-primary">Search</button>
</form>

<hr />

@if (results.Any())
{
    <table class="table table-bordered">
        <thead>
            <tr>
                <th>Patient Id</th>
                <th>UHID</th>
                <th>Name</th>
                <th>Mobile</th>
                <th>Action</th>
            </tr>
        </thead>
        <tbody>
        @foreach (var patient in results)
        {
            <tr>
                <td>@patient.Id</td>
                <td>@patient.UHID</td>
                <td>@patient.FullName</td>
                <td>@patient.MobileNumber</td>
                <td>
                    <a asp-action="PatientSummary" asp-route-patientId="@patient.Id" class="btn btn-sm btn-info">
                        View
                    </a>
                </td>
            </tr>
        }
        </tbody>
    </table>
}
'@

Write-Utf8File (Join-Path $viewsDir "RegisterPatient.cshtml") @'
@model Frontend.Models.Reception.RegisterPatientByReceptionRequestDto

<h2>Register Patient</h2>

<form asp-action="RegisterPatient" method="post">
    @Html.AntiForgeryToken()

    <div class="row">
        <div class="col-md-4 mb-3">
            <label asp-for="FirstName" class="form-label"></label>
            <input asp-for="FirstName" class="form-control" />
        </div>

        <div class="col-md-4 mb-3">
            <label asp-for="LastName" class="form-label"></label>
            <input asp-for="LastName" class="form-control" />
        </div>

        <div class="col-md-4 mb-3">
            <label asp-for="MobileNumber" class="form-label"></label>
            <input asp-for="MobileNumber" class="form-control" />
        </div>
    </div>

    <div class="mb-3">
        <label asp-for="DateOfBirth" class="form-label"></label>
        <input asp-for="DateOfBirth" type="date" class="form-control" />
    </div>

    <div class="mb-3">
        <label asp-for="Email" class="form-label"></label>
        <input asp-for="Email" class="form-control" />
    </div>

    <div class="form-check mb-2">
        <input asp-for="PortalAccessEnabled" class="form-check-input" />
        <label asp-for="PortalAccessEnabled" class="form-check-label"></label>
    </div>

    <div class="form-check mb-3">
        <input asp-for="SendPortalActivationSms" class="form-check-input" />
        <label asp-for="SendPortalActivationSms" class="form-check-label"></label>
    </div>

    <button type="submit" class="btn btn-success">Register</button>
</form>
'@

Write-Utf8File (Join-Path $viewsDir "BookAppointment.cshtml") @'
@model Frontend.Models.Reception.BookAppointmentRequestDto

<h2>Book Appointment</h2>

<form asp-action="BookAppointment" method="post">
    @Html.AntiForgeryToken()

    <div class="mb-3">
        <label asp-for="PatientId" class="form-label"></label>
        <input asp-for="PatientId" class="form-control" />
    </div>

    <div class="mb-3">
        <label asp-for="DoctorId" class="form-label"></label>
        <input asp-for="DoctorId" class="form-control" />
    </div>

    <div class="mb-3">
        <label asp-for="DepartmentId" class="form-label"></label>
        <input asp-for="DepartmentId" class="form-control" />
    </div>

    <div class="mb-3">
        <label asp-for="AppointmentDate" class="form-label"></label>
        <input asp-for="AppointmentDate" type="date" class="form-control" />
    </div>

    <div class="mb-3">
        <label asp-for="SlotStartTime" class="form-label"></label>
        <input asp-for="SlotStartTime" type="time" class="form-control" />
    </div>

    <div class="mb-3">
        <label asp-for="SlotEndTime" class="form-label"></label>
        <input asp-for="SlotEndTime" type="time" class="form-control" />
    </div>

    <div class="mb-3">
        <label asp-for="VisitType" class="form-label"></label>
        <input asp-for="VisitType" class="form-control" />
    </div>

    <div class="mb-3">
        <label asp-for="ReasonForVisit" class="form-label"></label>
        <textarea asp-for="ReasonForVisit" class="form-control"></textarea>
    </div>

    <div class="form-check mb-3">
        <input asp-for="IsTeleConsultation" class="form-check-input" />
        <label asp-for="IsTeleConsultation" class="form-check-label"></label>
    </div>

    <button type="submit" class="btn btn-primary">Book</button>
</form>
'@

Write-Utf8File (Join-Path $viewsDir "CheckIn.cshtml") @'
@model Frontend.Models.Reception.CheckInRequestDto

<h2>Patient Check-In</h2>

<form asp-action="CheckIn" method="post">
    @Html.AntiForgeryToken()

    <div class="mb-3">
        <label asp-for="AppointmentId" class="form-label"></label>
        <input asp-for="AppointmentId" class="form-control" />
    </div>

    <div class="mb-3">
        <label asp-for="PatientId" class="form-label"></label>
        <input asp-for="PatientId" class="form-control" />
    </div>

    <div class="mb-3">
        <label asp-for="DoctorId" class="form-label"></label>
        <input asp-for="DoctorId" class="form-control" />
    </div>

    <div class="mb-3">
        <label asp-for="DepartmentId" class="form-label"></label>
        <input asp-for="DepartmentId" class="form-control" />
    </div>

    <button type="submit" class="btn btn-success">Check In</button>
</form>
'@

Write-Utf8File (Join-Path $viewsDir "Queue.cshtml") @'
@model Frontend.Models.Reception.DepartmentQueueResponseDto

<h2>Department Queue</h2>

@if (Model == null)
{
    <div class="alert alert-warning">No queue data found.</div>
}
else
{
    <h5>@Model.DepartmentName - @Model.Date</h5>

    <table class="table table-striped">
        <thead>
            <tr>
                <th>Token</th>
                <th>Patient Id</th>
                <th>UHID</th>
                <th>Name</th>
                <th>Status</th>
            </tr>
        </thead>
        <tbody>
        @foreach (var item in Model.Queue)
        {
            <tr>
                <td>@item.TokenNumber</td>
                <td>@item.PatientId</td>
                <td>@item.UHID</td>
                <td>@item.PatientName</td>
                <td>@item.Status</td>
            </tr>
        }
        </tbody>
    </table>
}
'@

Write-Utf8File (Join-Path $viewsDir "PatientSummary.cshtml") @'
@model Frontend.Models.Reception.ReceptionPatientSummaryDto

<h2>Patient Summary</h2>

<table class="table table-bordered">
    <tr><th>Id</th><td>@Model.Id</td></tr>
    <tr><th>UHID</th><td>@Model.UHID</td></tr>
    <tr><th>Name</th><td>@Model.FullName</td></tr>
    <tr><th>Mobile</th><td>@Model.MobileNumber</td></tr>
    <tr><th>Email</th><td>@Model.Email</td></tr>
    <tr><th>DOB</th><td>@Model.DateOfBirth</td></tr>
</table>
'@

# -----------------------------
# Program.cs patch
# -----------------------------

$programPath = Join-Path $frontend "Program.cs"
if (Test-Path $programPath) {
    $programText = Get-Content $programPath -Raw

    if ($programText -notmatch 'AddHttpClient<IReceptionApiService,\s*ReceptionApiService>') {
        $registration = @'

builder.Services.AddHttpClient<IReceptionApiService, ReceptionApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7000/");
});
'@

        if ($programText -match 'builder\.Services\.AddHttpClient<IAppointmentApiService,\s*AppointmentApiService>\(client =>\s*\{.*?\}\);\s*') {
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
        Write-Host "Skipped Program.cs update: reception HttpClient already exists."
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

    if ($layoutText -notmatch 'asp-controller="Reception"') {
        $navBlock = @'

<li class="nav-item">
    <a class="nav-link text-dark" asp-controller="Reception" asp-action="SearchPatients">Reception</a>
</li>
<li class="nav-item">
    <a class="nav-link text-dark" asp-controller="Reception" asp-action="RegisterPatient">Register Patient</a>
</li>
<li class="nav-item">
    <a class="nav-link text-dark" asp-controller="Reception" asp-action="BookAppointment">Book Appointment</a>
</li>
<li class="nav-item">
    <a class="nav-link text-dark" asp-controller="Reception" asp-action="CheckIn">Check In</a>
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
        Write-Host "Skipped _Layout.cshtml update: reception nav already exists."
    }
}
else {
    Write-Warning "_Layout.cshtml not found, skipping nav update."
}

Write-Host ""
Write-Host "Reception frontend files added."
Write-Host "Now run:"
Write-Host "  dotnet build"
Write-Host "Then start:"
Write-Host "  - HealthcareGateway"
Write-Host "  - Hms.ReceptionApi"
Write-Host "  - Frontend"