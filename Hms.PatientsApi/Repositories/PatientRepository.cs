using Hms.PatientsApi.Data;
using Hms.PatientsApi.DTOs.Patients;
using Hms.PatientsApi.Entities;
using Hms.PatientsApi.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace Hms.PatientsApi.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly ApplicationDbContext _context;

    public PatientRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Patient?> GetByIdAsync(int id)
    {
        return await _context.Patients.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Patient?> GetByUhidAsync(string uhid)
    {
        return await _context.Patients.FirstOrDefaultAsync(x => x.UHID == uhid);
    }

    public async Task<bool> ExistsByMobileAsync(string mobileNumber, int? excludePatientId = null)
    {
        var query = _context.Patients.Where(x => x.MobileNumber == mobileNumber);

        if (excludePatientId.HasValue)
        {
            query = query.Where(x => x.Id != excludePatientId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task AddAsync(Patient patient)
    {
        await _context.Patients.AddAsync(patient);
    }

    public Task UpdateAsync(Patient patient)
    {
        _context.Patients.Update(patient);
        return Task.CompletedTask;
    }

    public async Task<PatientSearchResponseDto> SearchAsync(PatientSearchRequestDto request)
    {
        var query = _context.Patients.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.UHID))
            query = query.Where(x => x.UHID == request.UHID);

        if (!string.IsNullOrWhiteSpace(request.MobileNumber))
            query = query.Where(x => x.MobileNumber == request.MobileNumber);

        if (!string.IsNullOrWhiteSpace(request.Name))
            query = query.Where(x => x.FullName.Contains(request.Name));

        if (request.DateOfBirth.HasValue)
            query = query.Where(x => x.DateOfBirth == request.DateOfBirth.Value);

        var totalCount = await query.CountAsync();

        var patients = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new PatientResponseDto
            {
                Id = x.Id,
                UHID = x.UHID,
                FullName = x.FullName,
                DateOfBirth = x.DateOfBirth,
                Gender = x.Gender,
                MobileNumber = x.MobileNumber,
                Email = x.Email,
                BloodGroup = x.BloodGroup,
                PortalAccessEnabled = x.PortalAccessEnabled,
                PortalActivated = x.PortalActivated,
                Status = x.Status,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync();

        return new PatientSearchResponseDto
        {
            TotalCount = totalCount,
            Patients = patients
        };
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}