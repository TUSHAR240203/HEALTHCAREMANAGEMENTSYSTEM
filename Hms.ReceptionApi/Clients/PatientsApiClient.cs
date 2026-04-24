using System.Net;
using System.Net.Http.Json;
using Hms.ReceptionApi.DTOs.Reception;
using Hms.ReceptionApi.Interfaces.Clients;

namespace Hms.ReceptionApi.Clients;

public class PatientsApiClient : IPatientsApiClient
{
    private readonly HttpClient _httpClient;

    public PatientsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ReceptionPatientSearchResponseDto> SearchPatientsAsync(ReceptionPatientSearchRequestDto request)
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

        var response = await _httpClient.PostAsJsonAsync("/api/patients/search", patientSearchRequest);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to search patients. Details: {error}");
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
        var response = await _httpClient.GetAsync($"/api/patients/{patientId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to fetch patient summary. Details: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<PatientApiResponse>();

        return result == null ? null : MapPatientSummary(result);
    }

    public async Task<ReceptionPatientSummaryDto> RegisterPatientAsync(RegisterPatientByReceptionRequestDto request)
    {
        var patientCreateRequest = new
        {
<<<<<<< HEAD
            firstName = request.FirstName,
            middleName = request.MiddleName,
            lastName = request.LastName,
=======
            fullName = request.FullName,
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
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

        var response = await _httpClient.PostAsJsonAsync("/api/patients", patientCreateRequest);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to register patient. Details: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<PatientApiResponse>();

        return result == null
            ? throw new InvalidOperationException("Unable to parse patient registration response.")
            : MapPatientSummary(result);
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