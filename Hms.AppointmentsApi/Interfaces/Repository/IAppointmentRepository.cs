using Hms.AppointmentsApi.DTOs.Appointments;
using Hms.AppointmentsApi.Entities;

namespace Hms.AppointmentsApi.Interfaces.Repository;

public interface IAppointmentRepository
{
    Task AddAsync(Appointment appointment);
    Task<Appointment?> GetByIdAsync(int id);
    Task<List<Appointment>> GetByPatientIdAsync(int patientId);
    Task<List<Appointment>> GetByDoctorIdAsync(int doctorId);
    Task<bool> IsSlotBookedAsync(int doctorId, DateOnly appointmentDate, TimeOnly slotStartTime, TimeOnly slotEndTime, int? excludeAppointmentId = null);
    Task<AppointmentSearchResponseDto> SearchAsync(AppointmentSearchRequestDto request);
    Task UpdateAsync(Appointment appointment);
    Task SaveChangesAsync();
}