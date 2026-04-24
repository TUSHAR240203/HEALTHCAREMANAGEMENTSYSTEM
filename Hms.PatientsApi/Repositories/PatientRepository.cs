<<<<<<< HEAD
﻿using Hms.PatientsApi.Data;
=======
using Hms.PatientsApi.Data;
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
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
<<<<<<< HEAD
    {
        return await _context.Patients.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Patient?> GetByUhidAsync(string uhid)
    {
        return await _context.Patients.FirstOrDefaultAsync(x => x.UHID == uhid);
    }
=======
        => await _context.Patients.FirstOrDefaultAsync(x => x.Id == id);

    public async Task<Patient?> GetByUhidAsync(string uhid)
        => await _context.Patients.FirstOrDefaultAsync(x => x.UHID == uhid);
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85

    public async Task<bool> ExistsByMobileAsync(string mobileNumber, int? excludePatientId = null)
    {
        var query = _context.Patients.Where(x => x.MobileNumber == mobileNumber);

        if (excludePatientId.HasValue)
        {
            query = query.Where(x => x.Id != excludePatientId.Value);
        }

        return await query.AnyAsync();
    }

<<<<<<< HEAD
    public async Task AddAsync(Patient patient)
    {
        await _context.Patients.AddAsync(patient);
    }

=======
    public async Task<Patient?> GetByMobileAsync(string mobileNumber, int? excludePatientId = null)
    {
        var query = _context.Patients.Where(x => x.MobileNumber == mobileNumber);

        if (excludePatientId.HasValue)
        {
            query = query.Where(x => x.Id != excludePatientId.Value);
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task AddAsync(Patient patient)
        => await _context.Patients.AddAsync(patient);

>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
    public Task UpdateAsync(Patient patient)
    {
        _context.Patients.Update(patient);
        return Task.CompletedTask;
    }

<<<<<<< HEAD
=======
    public async Task AddMobileNumberChangeRequestAsync(MobileNumberChangeRequest request)
        => await _context.MobileNumberChangeRequests.AddAsync(request);

    public async Task<MobileNumberChangeRequest?> GetLatestPendingMobileChangeRequestAsync(int patientId, string mobileNumber)
    {
        var now = DateTime.UtcNow;

        return await _context.MobileNumberChangeRequests
            .Where(x => x.PatientId == patientId
                        && x.NewMobileNumber == mobileNumber
                        && !x.IsConsumed
                        && x.ExpiresAtUtc > now)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync();
    }

>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
    public async Task<PatientSearchResponseDto> SearchAsync(PatientSearchRequestDto request)
    {
        var query = _context.Patients.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.UHID))
            query = query.Where(x => x.UHID == request.UHID);

        if (!string.IsNullOrWhiteSpace(request.MobileNumber))
            query = query.Where(x => x.MobileNumber == request.MobileNumber);

        if (!string.IsNullOrWhiteSpace(request.Name))
<<<<<<< HEAD
            query = query.Where(x => x.FullName.Contains(request.Name));
=======
        {
            query = query.Where(x => ((x.FirstName ?? string.Empty) + " " + (x.MiddleName ?? string.Empty) + " " + (x.LastName ?? string.Empty)).Contains(request.Name));
        }
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85

        if (request.DateOfBirth.HasValue)
            query = query.Where(x => x.DateOfBirth == request.DateOfBirth.Value);

        var totalCount = await query.CountAsync();

        var patients = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
<<<<<<< HEAD
            .Select(x => new PatientResponseDto
            {
                Id = x.Id,
                UHID = x.UHID,
                FullName = x.FullName,
=======
            .ToListAsync();

        var patientDtos = patients
            .Select(x => new PatientResponseDto
            {
                Id = x.Id,
                PatientIdentifier = x.PatientIdentifier,
                UHID = x.UHID,
                FullName = string.Join(" ", new[] { x.FirstName, x.MiddleName, x.LastName }.Where(y => !string.IsNullOrWhiteSpace(y))),
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
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
<<<<<<< HEAD
            .ToListAsync();
=======
            .ToList();
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85

        return new PatientSearchResponseDto
        {
            TotalCount = totalCount,
<<<<<<< HEAD
            Patients = patients
=======
            Patients = patientDtos
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
        };
    }

    public async Task SaveChangesAsync()
<<<<<<< HEAD
    {
        await _context.SaveChangesAsync();
    }
}
=======
        => await _context.SaveChangesAsync();
}
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
