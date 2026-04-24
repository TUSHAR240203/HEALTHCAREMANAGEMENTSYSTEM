using Hms.DoctorsApi.DTOs.Doctors;
using Hms.DoctorsApi.Entities;

namespace Hms.DoctorsApi.Interfaces.Repository;

public interface IDoctorRepository
{
    Task AddAsync(Doctor doctor);
    Task<Doctor?> GetByIdAsync(int id);
    Task<List<Doctor>> SearchAsync(DoctorSearchRequestDto request);
    Task<bool> ExistsByDoctorCodeAsync(string doctorCode, int? excludeDoctorId = null);
    Task<bool> ExistsByLicenseNumberAsync(string licenseNumber, int? excludeDoctorId = null);
    Task UpdateAsync(Doctor doctor);
    Task SaveChangesAsync();

    Task AddScheduleAsync(DoctorSchedule schedule);
    Task<DoctorSchedule?> GetScheduleByIdAsync(int doctorId, int scheduleId);
    Task<List<DoctorSchedule>> GetSchedulesAsync(int doctorId);

    Task AddLeaveAsync(DoctorLeave leave);
    Task<DoctorLeave?> GetLeaveByIdAsync(int doctorId, int leaveId);
    Task<List<DoctorLeave>> GetLeavesAsync(int doctorId);
    Task<bool> HasLeaveOnDateAsync(int doctorId, DateOnly date);
}
