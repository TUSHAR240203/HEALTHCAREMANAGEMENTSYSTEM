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

    public DoctorService(
        IDoctorRepository doctorRepository,
        IAppointmentsApiClient appointmentsApiClient,
        IReceptionApiClient receptionApiClient)
    {
        _doctorRepository = doctorRepository;
        _appointmentsApiClient = appointmentsApiClient;
        _receptionApiClient = receptionApiClient;
    }

    public async Task<DoctorResponseDto> CreateAsync(CreateDoctorRequestDto request)
    {
        ValidateCreateRequest(request);

        var doctorCode = GenerateDoctorCode(request.FullName);
        var normalizedLicense = NormalizeNullable(request.LicenseNumber);

        var codeExists = await _doctorRepository.ExistsByDoctorCodeAsync(doctorCode);
        if (codeExists)
            doctorCode = $"{doctorCode}-{DateTime.UtcNow:HHmmss}";

        if (!string.IsNullOrWhiteSpace(normalizedLicense) && await _doctorRepository.ExistsByLicenseNumberAsync(normalizedLicense))
            throw new InvalidOperationException("A doctor with this license number already exists.");

        var doctor = new Doctor
        {
            DoctorCode = doctorCode,
            FullName = request.FullName.Trim(),
            Email = NormalizeNullable(request.Email),
            Phone = NormalizeNullable(request.Phone),
            Gender = NormalizeNullable(request.Gender),
            Qualification = NormalizeNullable(request.Qualification),
            Specialization = request.Specialization.Trim(),
            DepartmentId = request.DepartmentId,
            DepartmentName = request.DepartmentName.Trim(),
            ConsultationFee = request.ConsultationFee,
            ExperienceYears = request.ExperienceYears,
            LicenseNumber = normalizedLicense,
            RoomNumber = NormalizeNullable(request.RoomNumber),
            SupportsTeleConsultation = request.SupportsTeleConsultation,
            IsActive = true
        };

        await _doctorRepository.AddAsync(doctor);
        await _doctorRepository.SaveChangesAsync();

        return MapDoctor(doctor);
    }

    public async Task<DoctorResponseDto?> GetByIdAsync(int id)
    {
        if (id <= 0) throw new ArgumentException("Invalid doctor id.");
        var doctor = await _doctorRepository.GetByIdAsync(id);
        return doctor == null ? null : MapDoctor(doctor);
    }

    public async Task<List<DoctorResponseDto>> SearchAsync(DoctorSearchRequestDto request)
    {
        request ??= new DoctorSearchRequestDto();
        var doctors = await _doctorRepository.SearchAsync(request);
        return doctors.Select(MapDoctor).ToList();
    }

    public async Task<DoctorResponseDto?> UpdateAsync(int id, UpdateDoctorRequestDto request)
    {
        if (id <= 0) throw new ArgumentException("Invalid doctor id.");
        ValidateUpdateRequest(request);

        var doctor = await _doctorRepository.GetByIdAsync(id);
        if (doctor == null) return null;

        var normalizedLicense = NormalizeNullable(request.LicenseNumber);
        if (!string.IsNullOrWhiteSpace(normalizedLicense) && await _doctorRepository.ExistsByLicenseNumberAsync(normalizedLicense, id))
            throw new InvalidOperationException("Another doctor with this license number already exists.");

        doctor.FullName = request.FullName.Trim();
        doctor.Email = NormalizeNullable(request.Email);
        doctor.Phone = NormalizeNullable(request.Phone);
        doctor.Gender = NormalizeNullable(request.Gender);
        doctor.Qualification = NormalizeNullable(request.Qualification);
        doctor.Specialization = request.Specialization.Trim();
        doctor.DepartmentId = request.DepartmentId;
        doctor.DepartmentName = request.DepartmentName.Trim();
        doctor.ConsultationFee = request.ConsultationFee;
        doctor.ExperienceYears = request.ExperienceYears;
        doctor.LicenseNumber = normalizedLicense;
        doctor.RoomNumber = NormalizeNullable(request.RoomNumber);
        doctor.SupportsTeleConsultation = request.SupportsTeleConsultation;
        doctor.IsActive = request.IsActive;
        doctor.UpdatedAtUtc = DateTime.UtcNow;

        await _doctorRepository.UpdateAsync(doctor);
        await _doctorRepository.SaveChangesAsync();
        return MapDoctor(doctor);
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
        return schedules.Select(MapSchedule).ToList();
    }

    public async Task<DoctorScheduleResponseDto> AddScheduleAsync(int doctorId, CreateDoctorScheduleRequestDto request)
    {
        await EnsureDoctorExistsAsync(doctorId);
        ValidateScheduleRequest(request);

        var schedule = new DoctorSchedule
        {
            DoctorId = doctorId,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            BreakStartTime = request.BreakStartTime,
            BreakEndTime = request.BreakEndTime,
            SlotDurationMinutes = request.SlotDurationMinutes,
            MaxPatientsPerDay = request.MaxPatientsPerDay,
            IsActive = true
        };

        await _doctorRepository.AddScheduleAsync(schedule);
        await _doctorRepository.SaveChangesAsync();
        return MapSchedule(schedule);
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
        return leaves.Select(MapLeave).ToList();
    }

    public async Task<DoctorLeaveResponseDto> AddLeaveAsync(int doctorId, CreateDoctorLeaveRequestDto request)
    {
        await EnsureDoctorExistsAsync(doctorId);
        if (request.LeaveDate < DateOnly.FromDateTime(DateTime.UtcNow.Date))
            throw new ArgumentException("Leave date cannot be in the past.");

        if (await _doctorRepository.HasLeaveOnDateAsync(doctorId, request.LeaveDate))
            throw new InvalidOperationException("Doctor leave already exists for this date.");

        var leave = new DoctorLeave
        {
            DoctorId = doctorId,
            LeaveDate = request.LeaveDate,
            Reason = NormalizeNullable(request.Reason)
        };

        await _doctorRepository.AddLeaveAsync(leave);
        await _doctorRepository.SaveChangesAsync();
        return MapLeave(leave);
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
        {
            return new DoctorAvailabilityResponseDto
            {
                DoctorId = doctor.Id,
                DoctorName = doctor.FullName,
                Date = date,
                Slots = new List<DoctorAvailabilitySlotDto>()
            };
        }

        var schedules = await _doctorRepository.GetSchedulesAsync(doctorId);
        var schedule = schedules.FirstOrDefault(x => x.DayOfWeek == date.DayOfWeek && x.IsActive);
        if (schedule == null)
        {
            return new DoctorAvailabilityResponseDto
            {
                DoctorId = doctor.Id,
                DoctorName = doctor.FullName,
                Date = date,
                Slots = new List<DoctorAvailabilitySlotDto>()
            };
        }

        var slots = new List<DoctorAvailabilitySlotDto>();
        var current = schedule.StartTime;
        while (current.AddMinutes(schedule.SlotDurationMinutes) <= schedule.EndTime)
        {
            var slotEnd = current.AddMinutes(schedule.SlotDurationMinutes);
            var inBreak = schedule.BreakStartTime.HasValue && schedule.BreakEndTime.HasValue && current < schedule.BreakEndTime.Value && slotEnd > schedule.BreakStartTime.Value;
            if (!inBreak)
            {
                slots.Add(new DoctorAvailabilitySlotDto
                {
                    SlotStartTime = current,
                    SlotEndTime = slotEnd,
                    IsAvailable = true
                });
            }
            current = slotEnd;
        }

        return new DoctorAvailabilityResponseDto
        {
            DoctorId = doctor.Id,
            DoctorName = doctor.FullName,
            Date = date,
            Slots = slots
        };
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
        if (currentQueue != null && currentQueue.AppointmentId == appointmentId && currentQueue.Status == "Called")
        {
            await _receptionApiClient.StartQueueTokenAsync(currentQueue.QueueTokenId);
        }

        return await _appointmentsApiClient.StartAppointmentAsync(appointmentId);
    }

    public async Task<AppointmentResponseDto?> CompleteAppointmentAsync(int doctorId, int appointmentId, CompleteAppointmentRequestDto request)
    {
        await EnsureDoctorOwnsAppointmentAsync(doctorId, appointmentId);

        var currentQueue = await _receptionApiClient.GetDoctorCurrentQueueAsync(doctorId, DateOnly.FromDateTime(DateTime.UtcNow.Date));
        if (currentQueue != null && currentQueue.AppointmentId == appointmentId)
        {
            await _receptionApiClient.CompleteQueueTokenAsync(currentQueue.QueueTokenId, request.Notes);
        }

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

    private static void ValidateCreateRequest(CreateDoctorRequestDto request)
    {
        if (request == null) throw new ArgumentException("Request body is required.");
        if (string.IsNullOrWhiteSpace(request.FullName)) throw new ArgumentException("FullName is required.");
        if (string.IsNullOrWhiteSpace(request.Specialization)) throw new ArgumentException("Specialization is required.");
        if (request.DepartmentId <= 0) throw new ArgumentException("DepartmentId is required.");
        if (string.IsNullOrWhiteSpace(request.DepartmentName)) throw new ArgumentException("DepartmentName is required.");
        if (request.ConsultationFee < 0) throw new ArgumentException("ConsultationFee cannot be negative.");
        if (request.ExperienceYears < 0) throw new ArgumentException("ExperienceYears cannot be negative.");
    }

    private static void ValidateUpdateRequest(UpdateDoctorRequestDto request) => ValidateCreateRequest(request);

    private static void ValidateScheduleRequest(CreateDoctorScheduleRequestDto request)
    {
        if (request.EndTime <= request.StartTime)
            throw new ArgumentException("EndTime must be greater than StartTime.");
        if (request.SlotDurationMinutes <= 0)
            throw new ArgumentException("SlotDurationMinutes must be greater than zero.");
        if (request.BreakStartTime.HasValue != request.BreakEndTime.HasValue)
            throw new ArgumentException("Both break times are required together.");
        if (request.BreakStartTime.HasValue && request.BreakEndTime <= request.BreakStartTime)
            throw new ArgumentException("BreakEndTime must be greater than BreakStartTime.");
    }

    private static string GenerateDoctorCode(string fullName)
    {
        var letters = new string(fullName.Where(char.IsLetter).Take(4).ToArray()).ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(letters)) letters = "DOC";
        return $"DOC-{letters}";
    }

    private static string? NormalizeNullable(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static DoctorResponseDto MapDoctor(Doctor doctor) => new()
    {
        Id = doctor.Id,
        DoctorCode = doctor.DoctorCode,
        FullName = doctor.FullName,
        Email = doctor.Email,
        Phone = doctor.Phone,
        Gender = doctor.Gender,
        Qualification = doctor.Qualification,
        Specialization = doctor.Specialization,
        DepartmentId = doctor.DepartmentId,
        DepartmentName = doctor.DepartmentName,
        ConsultationFee = doctor.ConsultationFee,
        ExperienceYears = doctor.ExperienceYears,
        LicenseNumber = doctor.LicenseNumber,
        RoomNumber = doctor.RoomNumber,
        IsActive = doctor.IsActive,
        SupportsTeleConsultation = doctor.SupportsTeleConsultation,
        CreatedAtUtc = doctor.CreatedAtUtc
    };

    private static DoctorScheduleResponseDto MapSchedule(DoctorSchedule schedule) => new()
    {
        Id = schedule.Id,
        DoctorId = schedule.DoctorId,
        DayOfWeek = schedule.DayOfWeek,
        StartTime = schedule.StartTime,
        EndTime = schedule.EndTime,
        BreakStartTime = schedule.BreakStartTime,
        BreakEndTime = schedule.BreakEndTime,
        SlotDurationMinutes = schedule.SlotDurationMinutes,
        MaxPatientsPerDay = schedule.MaxPatientsPerDay,
        IsActive = schedule.IsActive
    };

    private static DoctorLeaveResponseDto MapLeave(DoctorLeave leave) => new()
    {
        Id = leave.Id,
        DoctorId = leave.DoctorId,
        LeaveDate = leave.LeaveDate,
        Reason = leave.Reason
    };
}
