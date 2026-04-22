using Hms.DoctorsApi.Data;
using Hms.DoctorsApi.DTOs.Doctors;
using Hms.DoctorsApi.Entities;
using Hms.DoctorsApi.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace Hms.DoctorsApi.Repositories;

public class DoctorRepository : IDoctorRepository
{
    private readonly DoctorsDbContext _context;

    public DoctorRepository(DoctorsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Doctor doctor) => await _context.Doctors.AddAsync(doctor);

    public async Task<Doctor?> GetByIdAsync(int id) => await _context.Doctors.FirstOrDefaultAsync(x => x.Id == id);

    public async Task<List<Doctor>> SearchAsync(DoctorSearchRequestDto request)
    {
        var query = _context.Doctors.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var name = request.Name.Trim();
            query = query.Where(x => x.FullName.Contains(name) || x.DoctorCode.Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(request.Specialization))
        {
            var specialization = request.Specialization.Trim();
            query = query.Where(x => x.Specialization.Contains(specialization));
        }

        if (request.DepartmentId.HasValue)
            query = query.Where(x => x.DepartmentId == request.DepartmentId.Value);

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        if (request.SupportsTeleConsultation.HasValue)
            query = query.Where(x => x.SupportsTeleConsultation == request.SupportsTeleConsultation.Value);

        return await query.OrderBy(x => x.FullName).ToListAsync();
    }

    public async Task<bool> ExistsByDoctorCodeAsync(string doctorCode, int? excludeDoctorId = null)
    {
        var query = _context.Doctors.Where(x => x.DoctorCode == doctorCode);
        if (excludeDoctorId.HasValue)
            query = query.Where(x => x.Id != excludeDoctorId.Value);
        return await query.AnyAsync();
    }

    public async Task<bool> ExistsByLicenseNumberAsync(string licenseNumber, int? excludeDoctorId = null)
    {
        var query = _context.Doctors.Where(x => x.LicenseNumber == licenseNumber);
        if (excludeDoctorId.HasValue)
            query = query.Where(x => x.Id != excludeDoctorId.Value);
        return await query.AnyAsync();
    }

    public Task UpdateAsync(Doctor doctor)
    {
        _context.Doctors.Update(doctor);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task AddScheduleAsync(DoctorSchedule schedule) => await _context.DoctorSchedules.AddAsync(schedule);

    public async Task<DoctorSchedule?> GetScheduleByIdAsync(int doctorId, int scheduleId)
        => await _context.DoctorSchedules.FirstOrDefaultAsync(x => x.DoctorId == doctorId && x.Id == scheduleId);

    public async Task<List<DoctorSchedule>> GetSchedulesAsync(int doctorId)
        => await _context.DoctorSchedules.Where(x => x.DoctorId == doctorId).OrderBy(x => x.DayOfWeek).ThenBy(x => x.StartTime).ToListAsync();

    public async Task AddLeaveAsync(DoctorLeave leave) => await _context.DoctorLeaves.AddAsync(leave);

    public async Task<DoctorLeave?> GetLeaveByIdAsync(int doctorId, int leaveId)
        => await _context.DoctorLeaves.FirstOrDefaultAsync(x => x.DoctorId == doctorId && x.Id == leaveId);

    public async Task<List<DoctorLeave>> GetLeavesAsync(int doctorId)
        => await _context.DoctorLeaves.Where(x => x.DoctorId == doctorId).OrderByDescending(x => x.LeaveDate).ToListAsync();

    public async Task<bool> HasLeaveOnDateAsync(int doctorId, DateOnly date)
        => await _context.DoctorLeaves.AnyAsync(x => x.DoctorId == doctorId && x.LeaveDate == date);
}
