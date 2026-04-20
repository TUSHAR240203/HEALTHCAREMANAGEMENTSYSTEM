using Hms.PatientsApi.DTOs.Patients;

namespace Hms.PatientsApi.Validators;

public static class PatientValidators
{
    public static List<string> ValidateCreate(CreatePatientRequestDto request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.FirstName))
            errors.Add("FirstName is required.");

        if (string.IsNullOrWhiteSpace(request.LastName))
            errors.Add("LastName is required.");

        if (string.IsNullOrWhiteSpace(request.MobileNumber))
            errors.Add("MobileNumber is required.");

        if (request.DateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
            errors.Add("DateOfBirth cannot be in the future.");

        if (!string.IsNullOrWhiteSpace(request.MobileNumber) && request.MobileNumber.Length < 10)
            errors.Add("MobileNumber must be valid.");

        return errors;
    }
}