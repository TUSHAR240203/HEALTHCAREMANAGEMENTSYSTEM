using AutoMapper;
using Hms.DoctorsApi.DTOs.Appointments;
using Hms.DoctorsApi.DTOs.Doctors;
using Hms.DoctorsApi.DTOs.Queue;
using Hms.DoctorsApi.Entities;
using Hms.DoctorsApi.Interfaces.Clients;
using Hms.DoctorsApi.Interfaces.Repository;
using Hms.DoctorsApi.Interfaces.Services;

namespace Hms.DoctorsApi.Services;

public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IAppointmentsApiClient _appointmentsApiClient;
    private readonly IReceptionApiClient _receptionApiClient;
    private readonly IMapper _mapper;

    public DoctorService(
        IDoctorRepository doctorRepository,
        IAppointmentsApiClient appointmentsApiClient,
        IReceptionApiClient receptionApiClient,
        IMapper mapper)
    {
        _doctorRepository = doctorRepository;
        _appointmentsApiClient = appointmentsApiClient;
        _receptionApiClient = receptionApiClient;
        _mapper = mapper;
    }

    public async Task<DoctorResponseDto> CreateAsync(CreateDoctorRequestDto request)
    {
        var doctorCode = GenerateDoctorCode(request.FullName);
        var normalizedLicense = NormalizeNullable(request.LicenseNumber);

        if (await _doctorRepository.ExistsByDoctorCodeAsync(doctorCode))
            doctorCode = $"{doctorCode}-{DateTime.UtcNow:HHmmss}";

        if (!string.IsNullOrWhiteSpace(normalizedLicense) && await _doctorRepository.ExistsByLicenseNumberAsync(normalizedLicense))
            throw new InvalidOperationException("A doctor with this license number already exists.");

        var doctor = _mapper.Map<Doctor>(request);
        doctor.DoctorCode = doctorCode;
        doctor.LicenseNumber = normalizedLicense;
        doctor.IsActive = true;

        await _doctorRepository.AddAsync(doctor);
        await _doctorRepository.SaveChangesAsync();

        return _mapper.Map<DoctorResponseDto>(doctor);
    }

    public async Task<DoctorResponseDto?> GetByIdAsync(int id)
    {
        if (id <= 0) throw new ArgumentException("Invalid doctor id.");
        var doctor = await _doctorRepository.GetByIdAsync(id);
        return doctor == null ? null : _mapper.Map<DoctorResponseDto>(doctor);
    }

    public async Task<List<DoctorResponseDto>> SearchAsync(DoctorSearchRequestDto request)
    {
        request ??= new DoctorSearchRequestDto();
        var doctors = await _doctorRepository.SearchAsync(request);
        return _mapper.Map<List<DoctorResponseDto>>(doctors);
    }

    public async Task<DoctorResponseDto?> UpdateAsync(int id, UpdateDoctorRequestDto request)
    {
        if (id <= 0) throw new ArgumentException("Invalid doctor id.");

        var doctor = await _doctorRepository.GetByIdAsync(id);
        if (doctor == null) return null;

        var normalizedLicense = NormalizeNullable(request.LicenseNumber);
        if (!string.IsNullOrWhiteSpace(normalizedLicense) && await _doctorRepository.ExistsByLicenseNumberAsync(normalizedLicense, id))
            throw new InvalidOperationException("Another doctor with this license number already exists.");

        _mapper.Map(request, doctor);
        doctor.LicenseNumber = normalizedLicense;
        doctor.UpdatedAtUtc = DateTime.UtcNow;

        await _doctorRepository.UpdateAsync(doctor);
        await _doctorRepository.SaveChangesAsync();
        return _mapper.Map<DoctorResponseDto>(doctor);
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        if (id <= 0) throw new ArgumentException("Invalid doctor id.");
        var doctor = await _doctorRepository.GetByIdAsync(id);
        if (doctor == null || doctor.IsDeleted) return false;

        doctor.IsDeleted = true;
        doctor.IsActive = false;
        doctor.UpdatedAtUtc = DateTime.UtcNow;
        await _doctorRepository.UpdateAsync(doctor);
        await _doctorRepository.SaveChangesAsync();
        return true;
    }

    public async Task<List<DoctorScheduleResponseDto>> GetSchedulesAsync(int doctorId)
    {
        await EnsureDoctorExistsAsync(doctorId);
        var schedules = await _doctorRepository.GetSchedulesAsync(doctorId);
        return _mapper.Map<List<DoctorScheduleResponseDto>>(schedules);
    }

    public async Task<DoctorScheduleResponseDto> AddScheduleAsync(int doctorId, CreateDoctorScheduleRequestDto request)
    {
        await EnsureDoctorExistsAsync(doctorId);

        var existingSchedules = await _doctorRepository.GetSchedulesAsync(doctorId);
        if (existingSchedules.Any(x => x.DayOfWeek == request.DayOfWeek && request.StartTime < x.EndTime && request.EndTime > x.StartTime))
            throw new InvalidOperationException("A schedule already exists for this doctor during the selected time range.");

        var schedule = _mapper.Map<DoctorSchedule>(request);
        schedule.DoctorId = doctorId;

        await _doctorRepository.AddScheduleAsync(schedule);
        await _doctorRepository.SaveChangesAsync();
        return _mapper.Map<DoctorScheduleResponseDto>(schedule);
    }

    public async Task<bool> DeleteScheduleAsync(int doctorId, int scheduleId)
    {
        var schedule = await _doctorRepository.GetScheduleByIdAsync(doctorId, scheduleId);
        if (schedule == null) return false;
        schedule.IsDeleted = true;
        schedule.UpdatedAtUtc = DateTime.UtcNow;
        await _doctorRepository.SaveChangesAsync();
        return true;
    }

    public async Task<List<DoctorLeaveResponseDto>> GetLeavesAsync(int doctorId)
    {
        await EnsureDoctorExistsAsync(doctorId);
        var leaves = await _doctorRepository.GetLeavesAsync(doctorId);
        return _mapper.Map<List<DoctorLeaveResponseDto>>(leaves);
    }

    public async Task<DoctorLeaveResponseDto> AddLeaveAsync(int doctorId, CreateDoctorLeaveRequestDto request)
    {
        await EnsureDoctorExistsAsync(doctorId);
        if (request.LeaveDate < DateOnly.FromDateTime(DateTime.UtcNow.Date))
            throw new ArgumentException("Leave date cannot be in the past.");

        if (await _doctorRepository.HasLeaveOnDateAsync(doctorId, request.LeaveDate))
            throw new InvalidOperationException("Doctor leave already exists for this date.");

        var leave = _mapper.Map<DoctorLeave>(request);
        leave.DoctorId = doctorId;

        await _doctorRepository.AddLeaveAsync(leave);
        await _doctorRepository.SaveChangesAsync();
        return _mapper.Map<DoctorLeaveResponseDto>(leave);
    }

    public async Task<bool> DeleteLeaveAsync(int doctorId, int leaveId)
    {
        var leave = await _doctorRepository.GetLeaveByIdAsync(doctorId, leaveId);
        if (leave == null) return false;
        leave.IsDeleted = true;
        leave.UpdatedAtUtc = DateTime.UtcNow;
        await _doctorRepository.SaveChangesAsync();
        return true;
    }

    public async Task<DoctorAvailabilityResponseDto> GetAvailableSlotsAsync(int doctorId, DateOnly date, bool? isTeleConsultation)
    {
        var doctor = await _doctorRepository.GetByIdAsync(doctorId) ?? throw new ArgumentException("Doctor not found.");
        if (!doctor.IsActive)
            throw new InvalidOperationException("Doctor is inactive.");
        if (isTeleConsultation == true && !doctor.SupportsTeleConsultation)
            throw new InvalidOperationException("Doctor does not support teleconsultation.");
        if (await _doctorRepository.HasLeaveOnDateAsync(doctorId, date))
            return BuildAvailabilityResponse(doctor, date, new List<DoctorAvailabilitySlotDto>());

        var schedules = await _doctorRepository.GetSchedulesAsync(doctorId);
        var schedule = schedules.FirstOrDefault(x => x.DayOfWeek == date.DayOfWeek && x.IsActive);
        if (schedule == null)
            return BuildAvailabilityResponse(doctor, date, new List<DoctorAvailabilitySlotDto>());

        var appointments = await _appointmentsApiClient.GetByDoctorIdAsync(doctorId);
        var activeAppointments = appointments
            .Where(x => x.AppointmentDate == date && x.Status != AppointmentStatus.Cancelled)
            .ToList();

        var maxReached = schedule.MaxPatientsPerDay.HasValue && activeAppointments.Count >= schedule.MaxPatientsPerDay.Value;
        var slots = new List<DoctorAvailabilitySlotDto>();
        var current = schedule.StartTime;

        while (current.AddMinutes(schedule.SlotDurationMinutes) <= schedule.EndTime)
        {
            var slotEnd = current.AddMinutes(schedule.SlotDurationMinutes);
            var inBreak = schedule.BreakStartTime.HasValue && schedule.BreakEndTime.HasValue && current < schedule.BreakEndTime.Value && slotEnd > schedule.BreakStartTime.Value;
            var alreadyBooked = activeAppointments.Any(x => current < x.SlotEndTime && slotEnd > x.SlotStartTime);

            if (!inBreak)
            {
                slots.Add(new DoctorAvailabilitySlotDto
                {
                    SlotStartTime = current,
                    SlotEndTime = slotEnd,
                    IsAvailable = !alreadyBooked && !maxReached
                });
            }
            current = slotEnd;
        }

        return BuildAvailabilityResponse(doctor, date, slots);
    }

    public async Task<List<AppointmentResponseDto>> GetTodayAppointmentsAsync(int doctorId)
    {
        await EnsureDoctorExistsAsync(doctorId);
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var appointments = await _appointmentsApiClient.GetByDoctorIdAsync(doctorId);

        return appointments
            .Where(x => x.AppointmentDate == today && x.Status != AppointmentStatus.Cancelled)
            .OrderBy(x => x.SlotStartTime)
            .ToList();
    }

    public async Task<List<AppointmentResponseDto>> GetUpcomingAppointmentsAsync(int doctorId)
    {
        await EnsureDoctorExistsAsync(doctorId);
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var appointments = await _appointmentsApiClient.GetByDoctorIdAsync(doctorId);

        return appointments
            .Where(x => x.AppointmentDate >= today && x.Status != AppointmentStatus.Cancelled && x.Status != AppointmentStatus.Completed)
            .OrderBy(x => x.AppointmentDate)
            .ThenBy(x => x.SlotStartTime)
            .ToList();
    }

    public async Task<DoctorQueueCurrentResponseDto?> GetCurrentQueueAsync(int doctorId, DateOnly date)
    {
        await EnsureDoctorExistsAsync(doctorId);
        return await _receptionApiClient.GetDoctorCurrentQueueAsync(doctorId, date);
    }

    public async Task<AppointmentResponseDto?> StartAppointmentAsync(int doctorId, int appointmentId)
    {
        await EnsureDoctorOwnsAppointmentAsync(doctorId, appointmentId);

        var currentQueue = await _receptionApiClient.GetDoctorCurrentQueueAsync(doctorId, DateOnly.FromDateTime(DateTime.UtcNow.Date));
        if (currentQueue != null && currentQueue.AppointmentId == appointmentId && currentQueue.Status is "Called" or "CheckedIn")
            await _receptionApiClient.StartQueueTokenAsync(currentQueue.QueueTokenId);

        return await _appointmentsApiClient.StartAppointmentAsync(appointmentId);
    }

    public async Task<AppointmentResponseDto?> CompleteAppointmentAsync(int doctorId, int appointmentId, CompleteAppointmentRequestDto request)
    {
        await EnsureDoctorOwnsAppointmentAsync(doctorId, appointmentId);

        var currentQueue = await _receptionApiClient.GetDoctorCurrentQueueAsync(doctorId, DateOnly.FromDateTime(DateTime.UtcNow.Date));
        if (currentQueue != null && currentQueue.AppointmentId == appointmentId)
            await _receptionApiClient.CompleteQueueTokenAsync(currentQueue.QueueTokenId, request.Notes);

        return await _appointmentsApiClient.CompleteAppointmentAsync(appointmentId, request);
    }

    public async Task<AppointmentResponseDto?> AddAppointmentNotesAsync(int doctorId, int appointmentId, UpdateAppointmentNotesRequestDto request)
    {
        await EnsureDoctorOwnsAppointmentAsync(doctorId, appointmentId);
        return await _appointmentsApiClient.AddAppointmentNotesAsync(appointmentId, request);
    }

    private async Task EnsureDoctorExistsAsync(int doctorId)
    {
        if (doctorId <= 0) throw new ArgumentException("Invalid doctor id.");
        var doctor = await _doctorRepository.GetByIdAsync(doctorId);
        if (doctor == null) throw new ArgumentException("Doctor not found.");
    }

    private async Task EnsureDoctorOwnsAppointmentAsync(int doctorId, int appointmentId)
    {
        await EnsureDoctorExistsAsync(doctorId);
        var appointments = await _appointmentsApiClient.GetByDoctorIdAsync(doctorId);
        if (!appointments.Any(x => x.Id == appointmentId))
            throw new ArgumentException("Appointment not found for this doctor.");
    }

    private static DoctorAvailabilityResponseDto BuildAvailabilityResponse(Doctor doctor, DateOnly date, List<DoctorAvailabilitySlotDto> slots) => new()
    {
        DoctorId = doctor.Id,
        DoctorName = doctor.FullName,
        Date = date,
        Slots = slots
    };

    private static string GenerateDoctorCode(string fullName)
    {
        var letters = new string(fullName.Where(char.IsLetter).Take(4).ToArray()).ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(letters)) letters = "DOC";
        return $"DOC-{letters}";
    }

    private static string? NormalizeNullable(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
