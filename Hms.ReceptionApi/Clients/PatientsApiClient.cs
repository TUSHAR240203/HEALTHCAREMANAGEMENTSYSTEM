using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Hms.ReceptionApi.DTOs.Reception;
using Hms.ReceptionApi.Interfaces.Clients;

namespace Hms.ReceptionApi.Clients;

public class PatientsApiClient : IPatientsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PatientsApiClient(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ReceptionPatientSearchResponseDto> SearchPatientsAsync(
        ReceptionPatientSearchRequestDto request)
    {
        var patientSearchRequest = new
        {
            uhid = request.UHID,
            mobileNumber = request.MobileNumber,
            name = request.Name,
            dateOfBirth = request.DateOfBirth,
            pageNumber = request.PageNumber,
            pageSize = request.PageSize
        };

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/patients/search");

        AddBearerToken(httpRequest);

        httpRequest.Content = JsonContent.Create(patientSearchRequest);

        using var response = await _httpClient.SendAsync(httpRequest);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Failed to search patients. Status: {(int)response.StatusCode}. Details: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<PatientsSearchApiResponse>();

        if (result == null)
            return new ReceptionPatientSearchResponseDto();

        return new ReceptionPatientSearchResponseDto
        {
            TotalCount = result.TotalCount,
            Patients = result.Patients.Select(MapPatientSummary).ToList()
        };
    }

    public async Task<ReceptionPatientSummaryDto?> GetPatientSummaryAsync(int patientId)
    {
        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/patients/{patientId}");

        AddBearerToken(httpRequest);

        using var response = await _httpClient.SendAsync(httpRequest);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Failed to fetch patient summary. Status: {(int)response.StatusCode}. Details: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<PatientApiResponse>();

        return result == null ? null : MapPatientSummary(result);
    }

    public async Task<ReceptionPatientSummaryDto> RegisterPatientAsync(
        RegisterPatientByReceptionRequestDto request)
    {
        var patientCreateRequest = new
        {
            fullName = request.FullName,
            dateOfBirth = request.DateOfBirth,
            gender = request.Gender,
            mobileNumber = request.MobileNumber,
            email = request.Email,
            bloodGroup = request.BloodGroup,
            addressLine1 = request.AddressLine1,
            city = request.City,
            state = request.State,
            country = request.Country,
            postalCode = request.PostalCode,
            emergencyContactName = request.EmergencyContactName,
            emergencyContactNumber = request.EmergencyContactNumber,
            emergencyContactRelation = request.EmergencyContactRelation,
            insuranceProvider = request.InsuranceProvider,
            insurancePolicyNumber = request.InsurancePolicyNumber,
            portalAccessEnabled = request.PortalAccessEnabled
        };

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/patients");

        AddBearerToken(httpRequest);

        httpRequest.Content = JsonContent.Create(patientCreateRequest);

        using var response = await _httpClient.SendAsync(httpRequest);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Failed to register patient. Status: {(int)response.StatusCode}. Details: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<PatientApiResponse>();

        return result == null
            ? throw new InvalidOperationException("Unable to parse patient registration response.")
            : MapPatientSummary(result);
    }

    private void AddBearerToken(HttpRequestMessage request)
    {
        var authHeader =
            _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authHeader))
            return;

        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return;

        var token = authHeader["Bearer ".Length..].Trim();

        if (string.IsNullOrWhiteSpace(token))
            return;

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    private static ReceptionPatientSummaryDto MapPatientSummary(PatientApiResponse patient)
    {
        return new ReceptionPatientSummaryDto
        {
            PatientId = patient.Id,
            UHID = patient.UHID,
            FullName = patient.FullName,
            DateOfBirth = patient.DateOfBirth,
            Gender = patient.Gender,
            MobileNumber = patient.MobileNumber,
            Email = patient.Email,
            PortalAccessEnabled = patient.PortalAccessEnabled,
            PortalActivated = patient.PortalActivated,
            Status = patient.Status,
            LastVisitDateUtc = null
        };
    }
}