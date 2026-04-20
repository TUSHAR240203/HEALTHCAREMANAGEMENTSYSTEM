using Hms.AuthApi.Entities;

namespace Hms.AuthApi.Interfaces.Services;

public interface IOtpService
{
    string GenerateOtp();
    OtpVerification CreateOtp(int patientId, string mobileNumber, string purpose);
}