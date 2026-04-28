using Hms.PatientsApi.DTOs.Patients;
using Hms.PatientsApi.Entities;
using Hms.PatientsApi.Enums;
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
        var normalizedMobile = request.MobileNumber.Trim();

        var patient = new Patient
        {
            PatientIdentifier = PatientIdentifierGenerator.Generate(),
            UHID = UhidGenerator.Generate(),

            FirstName = request.FirstName.Trim(),
            MiddleName = NormalizeNullable(request.MiddleName),
            LastName = request.LastName.Trim(),
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            MobileNumber = normalizedMobile,
            Email = NormalizeNullable(request.Email),

            PortalAccessEnabled = request.PortalAccessEnabled,
            PortalActivated = false,
            Status = PatientStatus.Active,
            IsProfileCompleted = false,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _patientRepository.AddAsync(patient);
        await _patientRepository.SaveChangesAsync();

        return MapToResponse(patient);
    }

    public async Task<PatientResponseDto?> CompleteProfileAsync(int id, CompletePatientProfileRequestDto request)
    {
        var patient = await _patientRepository.GetByIdAsync(id);
        if (patient == null) return null;

        patient.BloodGroup = NormalizeNullable(request.BloodGroup);
        patient.MaritalStatus = NormalizeNullable(request.MaritalStatus);
        patient.AddressLine1 = NormalizeNullable(request.AddressLine1);
        patient.AddressLine2 = NormalizeNullable(request.AddressLine2);
        patient.City = NormalizeNullable(request.City);
        patient.State = NormalizeNullable(request.State);
        patient.PostalCode = NormalizeNullable(request.PostalCode);
        patient.EmergencyContactName = NormalizeNullable(request.EmergencyContactName);
        patient.EmergencyContactNumber = NormalizeNullable(request.EmergencyContactNumber);
        patient.EmergencyContactRelation = NormalizeNullable(request.EmergencyContactRelation);
        patient.AadhaarNumber = NormalizeNullable(request.AadhaarNumber);
        patient.InsuranceProvider = NormalizeNullable(request.InsuranceProvider);
        patient.InsurancePolicyNumber = NormalizeNullable(request.InsurancePolicyNumber);

        patient.PhotoUrl = request.PhotoUrl;
        patient.IsProfileCompleted = true;
        patient.UpdatedAtUtc = DateTime.UtcNow;

        await _patientRepository.UpdateAsync(patient);
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

        var patient = await _patientRepository.GetByUhidAsync(uhid.Trim());
        return patient == null ? null : MapToResponse(patient);
    }

    public async Task<PatientResponseDto?> UpdateAsync(int id, UpdatePatientRequestDto request)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid patient id.");

        var patient = await _patientRepository.GetByIdAsync(id);
        if (patient == null)
            return null;

        var normalizedMobile = request.MobileNumber.Trim();
        var mobileOwner = await _patientRepository.GetByMobileAsync(normalizedMobile, id);

        if (mobileOwner != null)
        {
            var verifiedRequest =
                await _patientRepository.GetLatestPendingMobileChangeRequestAsync(id, normalizedMobile);

            if (verifiedRequest == null || !verifiedRequest.IsVerified)
            {
                throw new InvalidOperationException(
                    $"This mobile number is already linked with patient '{BuildFullName(mobileOwner.FirstName, mobileOwner.MiddleName, mobileOwner.LastName)}'. Send and verify OTP before updating.");
            }

            verifiedRequest.IsConsumed = true;
            verifiedRequest.UpdatedAtUtc = DateTime.UtcNow;
        }

        patient.FirstName = request.FirstName.Trim();
        patient.MiddleName = NormalizeNullable(request.MiddleName);
        patient.LastName = request.LastName.Trim();
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
        patient.PostalCode = NormalizeNullable(request.PostalCode);
        patient.EmergencyContactName = NormalizeNullable(request.EmergencyContactName);
        patient.EmergencyContactNumber = NormalizeNullable(request.EmergencyContactNumber);
        patient.EmergencyContactRelation = NormalizeNullable(request.EmergencyContactRelation);
        patient.AadhaarNumber = NormalizeNullable(request.AadhaarNumber);
        patient.InsuranceProvider = NormalizeNullable(request.InsuranceProvider);
        patient.InsurancePolicyNumber = NormalizeNullable(request.InsurancePolicyNumber);
        patient.PortalAccessEnabled = request.PortalAccessEnabled;
        patient.PortalActivated = request.PortalActivated;
        patient.Status = request.Status;
        patient.UpdatedAtUtc = DateTime.UtcNow;

        await _patientRepository.UpdateAsync(patient);
        await _patientRepository.SaveChangesAsync();

        return MapToResponse(patient);
    }

    public async Task<MobileNumberChangeOtpResponseDto> SendMobileNumberChangeOtpAsync(
        int patientId,
        RequestMobileNumberChangeOtpDto request)
    {
        var patient = await _patientRepository.GetByIdAsync(patientId)
            ?? throw new ArgumentException("Patient not found.");

        var normalizedMobile = request.MobileNumber.Trim();
        var owner = await _patientRepository.GetByMobileAsync(normalizedMobile, patientId);

        if (owner == null)
        {
            return new MobileNumberChangeOtpResponseDto
            {
                Message = "This mobile number is not linked with another account. You can update directly.",
                VerificationRequired = false
            };
        }

        var otp = new MobileNumberChangeRequest
        {
            PatientId = patientId,
            ExistingOwnerPatientId = owner.Id,
            NewMobileNumber = normalizedMobile,
            OtpCode = GenerateOtp(),
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5)
        };

        await _patientRepository.AddMobileNumberChangeRequestAsync(otp);
        await _patientRepository.SaveChangesAsync();

        Console.WriteLine($"[MOBILE CHANGE OTP SENT] PatientId={patientId}, Mobile={normalizedMobile}, OTP={otp.OtpCode}");

        return new MobileNumberChangeOtpResponseDto
        {
            Message = "This number is already linked with another account. OTP sent for verification.",
            VerificationRequired = true,
            ExpiresAtUtc = otp.ExpiresAtUtc
        };
    }

    public async Task<MobileNumberChangeOtpResponseDto> VerifyMobileNumberChangeOtpAsync(
        int patientId,
        VerifyMobileNumberChangeOtpDto request)
    {
        var normalizedMobile = request.MobileNumber.Trim();
        var verification =
            await _patientRepository.GetLatestPendingMobileChangeRequestAsync(patientId, normalizedMobile);

        if (verification == null || verification.OtpCode != request.OtpCode.Trim())
            throw new InvalidOperationException("Invalid or expired OTP.");

        verification.IsVerified = true;
        verification.VerifiedAtUtc = DateTime.UtcNow;
        verification.UpdatedAtUtc = DateTime.UtcNow;

        await _patientRepository.SaveChangesAsync();

        return new MobileNumberChangeOtpResponseDto
        {
            Message = "OTP verified successfully. You can now update the mobile number.",
            VerificationRequired = false
        };
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
        if (patient == null || patient.IsDeleted)
            return false;

        patient.IsDeleted = true;
        patient.UpdatedAtUtc = DateTime.UtcNow;

        await _patientRepository.UpdateAsync(patient);
        await _patientRepository.SaveChangesAsync();

        return true;
    }

    private static string GenerateOtp()
        => Random.Shared.Next(100000, 999999).ToString();

    private static string BuildFullName(string firstName, string? middleName, string lastName)
        => string.Join(" ", new[] { firstName?.Trim(), middleName?.Trim(), lastName?.Trim() }
            .Where(x => !string.IsNullOrWhiteSpace(x)));

    private static string? NormalizeNullable(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static PatientResponseDto MapToResponse(Patient patient)
    {
        return new PatientResponseDto
        {
            Id = patient.Id,
            PatientIdentifier = patient.PatientIdentifier,
            UHID = patient.UHID,
            FullName = BuildFullName(patient.FirstName, patient.MiddleName, patient.LastName),
            DateOfBirth = patient.DateOfBirth,
            Gender = patient.Gender,
            MobileNumber = patient.MobileNumber,
            Email = patient.Email,
            BloodGroup = patient.BloodGroup,
            MaritalStatus = patient.MaritalStatus,
            AddressLine1 = patient.AddressLine1,
            AddressLine2 = patient.AddressLine2,
            City = patient.City,
            State = patient.State,
            PostalCode = patient.PostalCode,
            EmergencyContactName = patient.EmergencyContactName,
            EmergencyContactNumber = patient.EmergencyContactNumber,
            EmergencyContactRelation = patient.EmergencyContactRelation,
            AadhaarNumber = patient.AadhaarNumber,
            InsuranceProvider = patient.InsuranceProvider,
            InsurancePolicyNumber = patient.InsurancePolicyNumber,
            PortalAccessEnabled = patient.PortalAccessEnabled,
            PortalActivated = patient.PortalActivated,
            Status = patient.Status,
            CreatedAtUtc = patient.CreatedAtUtc,
            IsProfileCompleted = patient.IsProfileCompleted,
            PhotoUrl = patient.PhotoUrl
        };
    }
}
