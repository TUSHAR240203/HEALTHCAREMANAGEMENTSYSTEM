using Hms.PatientsApi.DTOs.Patients;
using Hms.PatientsApi.Entities;
using Hms.PatientsApi.Helpers;
using Hms.PatientsApi.Interfaces.Repository;
using Hms.PatientsApi.Interfaces.Services;

namespace Hms.PatientsApi.Services;

public class PatientService : IPatientService
{
    private readonly IPatientRepository _patientRepository;

    public PatientService(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<PatientResponseDto> CreateAsync(CreatePatientRequestDto request)
    {
        ValidateCreateRequest(request);

        var normalizedMobile = request.MobileNumber.Trim();

        var mobileExists = await _patientRepository.ExistsByMobileAsync(normalizedMobile);
        if (mobileExists)
        {
            throw new InvalidOperationException("A patient with this mobile number already exists.");
        }

        var patient = new Patient
        {
            UHID = UhidGenerator.Generate(),
            FirstName = request.FirstName.Trim(),
            MiddleName = NormalizeNullable(request.MiddleName),
            LastName = request.LastName.Trim(),
            FullName = BuildFullName(request.FirstName, request.MiddleName, request.LastName),
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            MobileNumber = normalizedMobile,
            Email = NormalizeNullable(request.Email),
            BloodGroup = NormalizeNullable(request.BloodGroup),
            MaritalStatus = NormalizeNullable(request.MaritalStatus),
            AddressLine1 = NormalizeNullable(request.AddressLine1),
            AddressLine2 = NormalizeNullable(request.AddressLine2),
            City = NormalizeNullable(request.City),
            State = NormalizeNullable(request.State),
            Country = NormalizeNullable(request.Country),
            PostalCode = NormalizeNullable(request.PostalCode),
            EmergencyContactName = NormalizeNullable(request.EmergencyContactName),
            EmergencyContactNumber = NormalizeNullable(request.EmergencyContactNumber),
            EmergencyContactRelation = NormalizeNullable(request.EmergencyContactRelation),
            AadhaarNumber = NormalizeNullable(request.AadhaarNumber),
            InsuranceProvider = NormalizeNullable(request.InsuranceProvider),
            InsurancePolicyNumber = NormalizeNullable(request.InsurancePolicyNumber),
            PortalAccessEnabled = request.PortalAccessEnabled,
            PortalActivated =false
        };

        await _patientRepository.AddAsync(patient);
        await _patientRepository.SaveChangesAsync();

        return MapToResponse(patient);
    }

    public async Task<PatientResponseDto?> GetByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid patient id.");

        var patient = await _patientRepository.GetByIdAsync(id);
        return patient == null ? null : MapToResponse(patient);
    }

    public async Task<PatientResponseDto?> GetByUhidAsync(string uhid)
    {
        if (string.IsNullOrWhiteSpace(uhid))
            throw new ArgumentException("UHID is required.");

        var normalizedUhid = uhid.Trim();

        var patient = await _patientRepository.GetByUhidAsync(normalizedUhid);
        return patient == null ? null : MapToResponse(patient);
    }

    public async Task<PatientResponseDto?> UpdateAsync(int id, UpdatePatientRequestDto request)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid patient id.");

        ValidateUpdateRequest(request);

        var patient = await _patientRepository.GetByIdAsync(id);
        if (patient == null)
            return null;

        var normalizedMobile = request.MobileNumber.Trim();

        var mobileExists = await _patientRepository.ExistsByMobileAsync(normalizedMobile, id);
        if (mobileExists)
        {
            throw new InvalidOperationException("Another patient with this mobile number already exists.");
        }

        patient.FirstName = request.FirstName.Trim();
        patient.MiddleName = NormalizeNullable(request.MiddleName);
        patient.LastName = request.LastName.Trim();
        patient.FullName = BuildFullName(request.FirstName, request.MiddleName, request.LastName);
        patient.DateOfBirth = request.DateOfBirth;
        patient.Gender = request.Gender;
        patient.MobileNumber = normalizedMobile;
        patient.Email = NormalizeNullable(request.Email);
        patient.BloodGroup = NormalizeNullable(request.BloodGroup);
        patient.MaritalStatus = NormalizeNullable(request.MaritalStatus);
        patient.AddressLine1 = NormalizeNullable(request.AddressLine1);
        patient.AddressLine2 = NormalizeNullable(request.AddressLine2);
        patient.City = NormalizeNullable(request.City);
        patient.State = NormalizeNullable(request.State);
        patient.Country = NormalizeNullable(request.Country);
        patient.PostalCode = NormalizeNullable(request.PostalCode);
        patient.EmergencyContactName = NormalizeNullable(request.EmergencyContactName);
        patient.EmergencyContactNumber = NormalizeNullable(request.EmergencyContactNumber);
        patient.EmergencyContactRelation = NormalizeNullable(request.EmergencyContactRelation);
        patient.AadhaarNumber = NormalizeNullable(request.AadhaarNumber);
        patient.InsuranceProvider = NormalizeNullable(request.InsuranceProvider);
        patient.InsurancePolicyNumber = NormalizeNullable(request.InsurancePolicyNumber);
        patient.PortalAccessEnabled = request.PortalAccessEnabled;
        patient.Status = request.Status;
        patient.UpdatedAtUtc = DateTime.UtcNow;
        patient.PortalActivated = request.PortalActivated;
        await _patientRepository.UpdateAsync(patient);
        await _patientRepository.SaveChangesAsync();

        return MapToResponse(patient);
    }

    public async Task<PatientSearchResponseDto> SearchAsync(PatientSearchRequestDto request)
    {
        if (request == null)
            throw new ArgumentException("Search request is required.");

        request.PageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        request.PageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        request.PageSize = request.PageSize > 100 ? 100 : request.PageSize;

        request.UHID = NormalizeNullable(request.UHID);
        request.MobileNumber = NormalizeNullable(request.MobileNumber);
        request.Name = NormalizeNullable(request.Name);

        return await _patientRepository.SearchAsync(request);
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid patient id.");

        var patient = await _patientRepository.GetByIdAsync(id);
        if (patient == null)
            return false;

        if (patient.IsDeleted)
            return false;

        patient.IsDeleted = true;
        patient.UpdatedAtUtc = DateTime.UtcNow;

        await _patientRepository.UpdateAsync(patient);
        await _patientRepository.SaveChangesAsync();

        return true;
    }

    private static void ValidateCreateRequest(CreatePatientRequestDto request)
    {
        if (request == null)
            throw new ArgumentException("Request body is required.");

        if (string.IsNullOrWhiteSpace(request.FirstName))
            throw new ArgumentException("First name is required.");

        if (string.IsNullOrWhiteSpace(request.LastName))
            throw new ArgumentException("Last name is required.");

        if (string.IsNullOrWhiteSpace(request.MobileNumber))
            throw new ArgumentException("Mobile number is required.");

        if (request.DateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("Date of birth cannot be in the future.");

        if (!IsValidMobile(request.MobileNumber))
            throw new ArgumentException("Mobile number must be 10 digits.");
    }

    private static void ValidateUpdateRequest(UpdatePatientRequestDto request)
    {
        if (request == null)
            throw new ArgumentException("Request body is required.");

        if (string.IsNullOrWhiteSpace(request.FirstName))
            throw new ArgumentException("First name is required.");

        if (string.IsNullOrWhiteSpace(request.LastName))
            throw new ArgumentException("Last name is required.");

        if (string.IsNullOrWhiteSpace(request.MobileNumber))
            throw new ArgumentException("Mobile number is required.");

        if (request.DateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("Date of birth cannot be in the future.");

        if (!IsValidMobile(request.MobileNumber))
            throw new ArgumentException("Mobile number must be 10 digits.");
    }

    private static bool IsValidMobile(string mobileNumber)
    {
        var trimmed = mobileNumber.Trim();
        return trimmed.Length == 10 && trimmed.All(char.IsDigit);
    }

    private static string BuildFullName(string firstName, string? middleName, string lastName)
    {
        return string.Join(" ", new[]
        {
            firstName?.Trim(),
            middleName?.Trim(),
            lastName?.Trim()
        }.Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    private static string? NormalizeNullable(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static PatientResponseDto MapToResponse(Patient patient)
    {
        return new PatientResponseDto
        {
            Id = patient.Id,
            UHID = patient.UHID,
            FullName = patient.FullName,
            DateOfBirth = patient.DateOfBirth,
            Gender = patient.Gender,
            MobileNumber = patient.MobileNumber,
            Email = patient.Email,
            BloodGroup = patient.BloodGroup,
            PortalAccessEnabled = patient.PortalAccessEnabled,
            PortalActivated = patient.PortalActivated,
            Status = patient.Status,
            CreatedAtUtc = patient.CreatedAtUtc
        };
    }
}