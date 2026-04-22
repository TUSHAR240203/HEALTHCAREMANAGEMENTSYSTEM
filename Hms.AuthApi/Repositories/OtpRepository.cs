using Hms.AuthApi.Data;
using Hms.AuthApi.Entities;
using Hms.AuthApi.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace Hms.AuthApi.Repositories;

public class OtpRepository : IOtpRepository
{
    private readonly AuthDbContext _context;

    public OtpRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(OtpVerification otp)
    {
        await _context.OtpVerifications.AddAsync(otp);
    }

    public async Task<OtpVerification?> GetValidOtpAsync(int patientId, string mobileNumber, string otpCode, string purpose)
    {
        var now = DateTime.UtcNow;

        return await _context.OtpVerifications
            .Where(x =>
                x.PatientId == patientId &&
                x.MobileNumber == mobileNumber &&
                x.OtpCode == otpCode &&
                x.Purpose == purpose &&
                !x.IsUsed &&
                x.ExpiresAtUtc > now &&
                !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync();
    }


    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}