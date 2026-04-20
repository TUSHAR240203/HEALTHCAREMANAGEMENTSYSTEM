using Hms.AppointmentsApi.Data;
using Hms.AppointmentsApi.DTOs.Appointments;
using Hms.AppointmentsApi.Entities;
using Hms.AppointmentsApi.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace Hms.AppointmentsApi.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly AppointmentsDbContext _context;

    public AppointmentRepository(AppointmentsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Appointment appointment)
    {
        await _context.Appointments.AddAsync(appointment);
    }

    public async Task<Appointment?> GetByIdAsync(int id)
    {
        return await _context.Appointments.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Appointment>> GetByPatientIdAsync(int patientId)
    {
        return await _context.Appointments
            .Where(x => x.PatientId == patientId)
            .OrderByDescending(x => x.AppointmentDate)
            .ThenByDescending(x => x.SlotStartTime)
            .ToListAsync();
    }

    public async Task<List<Appointment>> GetByDoctorIdAsync(int doctorId)
    {
        return await _context.Appointments
            .Where(x => x.DoctorId == doctorId)
            .OrderByDescending(x => x.AppointmentDate)
            .ThenByDescending(x => x.SlotStartTime)
            .ToListAsync();
    }

    public async Task<bool> IsSlotBookedAsync(int doctorId, DateOnly appointmentDate, TimeOnly slotStartTime, TimeOnly slotEndTime, int? excludeAppointmentId = null)
    {
        var query = _context.Appointments.Where(x =>
            x.DoctorId == doctorId &&
            x.AppointmentDate == appointmentDate &&
            x.Status != Enums.AppointmentStatus.Cancelled &&
            (
                (slotStartTime >= x.SlotStartTime && slotStartTime < x.SlotEndTime) ||
                (slotEndTime > x.SlotStartTime && slotEndTime <= x.SlotEndTime) ||
                (slotStartTime <= x.SlotStartTime && slotEndTime >= x.SlotEndTime)
            ));

        if (excludeAppointmentId.HasValue)
        {
            query = query.Where(x => x.Id != excludeAppointmentId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<AppointmentSearchResponseDto> SearchAsync(AppointmentSearchRequestDto request)
    {
        var query = _context.Appointments.AsQueryable();

        if (request.PatientId.HasValue)
            query = query.Where(x => x.PatientId == request.PatientId.Value);

        if (request.DoctorId.HasValue)
            query = query.Where(x => x.DoctorId == request.DoctorId.Value);

        if (request.DepartmentId.HasValue)
            query = query.Where(x => x.DepartmentId == request.DepartmentId.Value);

        if (request.AppointmentDate.HasValue)
            query = query.Where(x => x.AppointmentDate == request.AppointmentDate.Value);

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        var totalCount = await query.CountAsync();

        var appointments = await query
            .OrderByDescending(x => x.AppointmentDate)
            .ThenByDescending(x => x.SlotStartTime)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new AppointmentResponseDto
            {
                Id = x.Id,
                PatientId = x.PatientId,
                UHID = x.UHID,
                DoctorId = x.DoctorId,
                DoctorName = x.DoctorName,
                DepartmentId = x.DepartmentId,
                DepartmentName = x.DepartmentName,
                AppointmentDate = x.AppointmentDate,
                SlotStartTime = x.SlotStartTime,
                SlotEndTime = x.SlotEndTime,
                VisitType = x.VisitType,
                ReasonForVisit = x.ReasonForVisit,
                IsTeleConsultation = x.IsTeleConsultation,
                Status = x.Status,
                CancellationReason = x.CancellationReason,
                CompletionNotes = x.CompletionNotes,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync();

        return new AppointmentSearchResponseDto
        {
            TotalCount = totalCount,
            Appointments = appointments
        };
    }

    public Task UpdateAsync(Appointment appointment)
    {
        _context.Appointments.Update(appointment);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}