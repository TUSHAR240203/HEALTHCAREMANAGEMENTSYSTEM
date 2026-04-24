using Hms.AuthApi.Entities;
using Hms.AuthApi.Interfaces.Services;

namespace Hms.AuthApi.Services;

public class OtpService : IOtpService
{
    public string GenerateOtp()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    public OtpVerification CreateOtp(int patientId, string mobileNumber, string purpose)
    {
        return new OtpVerification
        {
            PatientId = patientId,
            MobileNumber = mobileNumber,
            OtpCode = GenerateOtp(),
            Purpose = purpose,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false
        };
    }
<<<<<<< HEAD
=======

>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
}