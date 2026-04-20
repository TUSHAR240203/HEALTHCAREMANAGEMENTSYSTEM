using Hms.AuthApi.Entities;

namespace Hms.AuthApi.Interfaces.Repository;

public interface IOtpRepository
{
    Task AddAsync(OtpVerification otp);
    Task<OtpVerification?> GetValidOtpAsync(int patientId, string mobileNumber, string otpCode, string purpose);
    Task SaveChangesAsync();
}