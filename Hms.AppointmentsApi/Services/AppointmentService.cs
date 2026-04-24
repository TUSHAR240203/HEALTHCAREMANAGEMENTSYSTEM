using Hms.AppointmentsApi.DTOs.Appointments;
<<<<<<< HEAD
using Hms.AppointmentsApi.DTOs.Doctors;
using Hms.AppointmentsApi.Entities;
using Hms.AppointmentsApi.Enums;
using Hms.AppointmentsApi.Interfaces.Clients;
=======
using Hms.AppointmentsApi.Entities;
using Hms.AppointmentsApi.Enums;
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
using Hms.AppointmentsApi.Interfaces.Repository;
using Hms.AppointmentsApi.Interfaces.Services;

namespace Hms.AppointmentsApi.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
<<<<<<< HEAD
    private readonly IDoctorsApiClient _doctorsApiClient;

    public AppointmentService(IAppointmentRepository appointmentRepository, IDoctorsApiClient doctorsApiClient)
    {
        _appointmentRepository = appointmentRepository;
        _doctorsApiClient = doctorsApiClient;
=======

    public AppointmentService(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
    }

    public async Task<AppointmentResponseDto> CreateAsync(CreateAppointmentRequestDto request)
    {
        ValidateCreateRequest(request);

<<<<<<< HEAD
        var doctor = await ValidateDoctorAsync(
            request.DoctorId,
            request.DepartmentId,
            request.IsTeleConsultation,
            request.AppointmentDate,
            request.SlotStartTime,
            request.SlotEndTime);

=======
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
        var slotBooked = await _appointmentRepository.IsSlotBookedAsync(
            request.DoctorId,
            request.AppointmentDate,
            request.SlotStartTime,
            request.SlotEndTime);

        if (slotBooked)
            throw new InvalidOperationException("This slot is already booked for the doctor.");

        var appointment = new Appointment
        {
            PatientId = request.PatientId,
            UHID = request.UHID.Trim(),
<<<<<<< HEAD
            DoctorId = doctor.Id,
            DoctorName = doctor.FullName,
            DepartmentId = doctor.DepartmentId,
            DepartmentName = doctor.DepartmentName,
=======
            DoctorId = request.DoctorId,
            DoctorName = NormalizeNullable(request.DoctorName),
            DepartmentId = request.DepartmentId,
            DepartmentName = NormalizeNullable(request.DepartmentName),
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
            AppointmentDate = request.AppointmentDate,
            SlotStartTime = request.SlotStartTime,
            SlotEndTime = request.SlotEndTime,
            VisitType = request.VisitType.Trim(),
            ReasonForVisit = NormalizeNullable(request.ReasonForVisit),
            IsTeleConsultation = request.IsTeleConsultation,
            Status = AppointmentStatus.Booked
        };

        await _appointmentRepository.AddAsync(appointment);
        await _appointmentRepository.SaveChangesAsync();

        return MapToResponse(appointment);
    }

    public async Task<AppointmentResponseDto?> GetByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid appointment id.");

        var appointment = await _appointmentRepository.GetByIdAsync(id);
        return appointment == null ? null : MapToResponse(appointment);
    }

    public async Task<List<AppointmentResponseDto>> GetByPatientIdAsync(int patientId)
    {
        if (patientId <= 0)
            throw new ArgumentException("Invalid patient id.");

        var appointments = await _appointmentRepository.GetByPatientIdAsync(patientId);
        return appointments.Select(MapToResponse).ToList();
    }

    public async Task<List<AppointmentResponseDto>> GetByDoctorIdAsync(int doctorId)
    {
        if (doctorId <= 0)
            throw new ArgumentException("Invalid doctor id.");

        var appointments = await _appointmentRepository.GetByDoctorIdAsync(doctorId);
        return appointments.Select(MapToResponse).ToList();
    }

    public async Task<AppointmentSearchResponseDto> SearchAsync(AppointmentSearchRequestDto request)
    {
        if (request == null)
            throw new ArgumentException("Search request is required.");

        request.PageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        request.PageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        request.PageSize = request.PageSize > 100 ? 100 : request.PageSize;

        return await _appointmentRepository.SearchAsync(request);
    }

    public async Task<AppointmentResponseDto?> RescheduleAsync(int id, RescheduleAppointmentRequestDto request)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid appointment id.");

        ValidateRescheduleRequest(request);

        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null)
            return null;

        if (appointment.Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Cancelled appointment cannot be rescheduled.");

        if (appointment.Status == AppointmentStatus.Completed)
            throw new InvalidOperationException("Completed appointment cannot be rescheduled.");

<<<<<<< HEAD
        await ValidateDoctorAsync(
            appointment.DoctorId,
            appointment.DepartmentId,
            appointment.IsTeleConsultation,
            request.NewAppointmentDate,
            request.NewSlotStartTime,
            request.NewSlotEndTime);

=======
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
        var slotBooked = await _appointmentRepository.IsSlotBookedAsync(
            appointment.DoctorId,
            request.NewAppointmentDate,
            request.NewSlotStartTime,
            request.NewSlotEndTime,
            appointment.Id);

        if (slotBooked)
            throw new InvalidOperationException("This new slot is already booked for the doctor.");

        appointment.AppointmentDate = request.NewAppointmentDate;
        appointment.SlotStartTime = request.NewSlotStartTime;
        appointment.SlotEndTime = request.NewSlotEndTime;
        appointment.Status = AppointmentStatus.Rescheduled;
        appointment.UpdatedAtUtc = DateTime.UtcNow;

        await _appointmentRepository.UpdateAsync(appointment);
        await _appointmentRepository.SaveChangesAsync();

        return MapToResponse(appointment);
    }

    public async Task<AppointmentResponseDto?> CancelAsync(int id, CancelAppointmentRequestDto request)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid appointment id.");

        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null)
            return null;

        if (appointment.Status == AppointmentStatus.Completed)
            throw new InvalidOperationException("Completed appointment cannot be cancelled.");

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancellationReason = NormalizeNullable(request.Reason);
        appointment.UpdatedAtUtc = DateTime.UtcNow;

        await _appointmentRepository.UpdateAsync(appointment);
        await _appointmentRepository.SaveChangesAsync();

        return MapToResponse(appointment);
    }

<<<<<<< HEAD
    public async Task<AppointmentResponseDto?> StartAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid appointment id.");

        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null)
            return null;

        if (appointment.Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Cancelled appointment cannot be started.");

        if (appointment.Status == AppointmentStatus.Completed)
            throw new InvalidOperationException("Completed appointment cannot be started.");

        appointment.Status = AppointmentStatus.InConsultation;
        appointment.UpdatedAtUtc = DateTime.UtcNow;

        await _appointmentRepository.UpdateAsync(appointment);
        await _appointmentRepository.SaveChangesAsync();

        return MapToResponse(appointment);
    }

=======
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
    public async Task<AppointmentResponseDto?> CompleteAsync(int id, CompleteAppointmentRequestDto request)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid appointment id.");

        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null)
            return null;

        if (appointment.Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Cancelled appointment cannot be completed.");

        appointment.Status = AppointmentStatus.Completed;
        appointment.CompletionNotes = NormalizeNullable(request.Notes);
        appointment.UpdatedAtUtc = DateTime.UtcNow;

        await _appointmentRepository.UpdateAsync(appointment);
        await _appointmentRepository.SaveChangesAsync();

        return MapToResponse(appointment);
    }

<<<<<<< HEAD
    public async Task<AppointmentResponseDto?> AddNotesAsync(int id, UpdateAppointmentNotesRequestDto request)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid appointment id.");

        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null)
            return null;

        if (appointment.Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Cancelled appointment cannot be updated.");

        appointment.CompletionNotes = NormalizeNullable(request.Notes);
        appointment.UpdatedAtUtc = DateTime.UtcNow;

        await _appointmentRepository.UpdateAsync(appointment);
        await _appointmentRepository.SaveChangesAsync();

        return MapToResponse(appointment);
    }

    private async Task<DoctorSummaryDto> ValidateDoctorAsync(int doctorId, int departmentId, bool isTeleConsultation, DateOnly appointmentDate, TimeOnly slotStartTime, TimeOnly slotEndTime)
    {
        var doctor = await _doctorsApiClient.GetDoctorByIdAsync(doctorId);
        if (doctor == null)
            throw new ArgumentException("Doctor not found.");

        if (!doctor.IsActive)
            throw new InvalidOperationException("Doctor is inactive.");

        if (doctor.DepartmentId != departmentId)
            throw new InvalidOperationException("Selected doctor does not belong to the provided department.");

        if (isTeleConsultation && !doctor.SupportsTeleConsultation)
            throw new InvalidOperationException("Selected doctor does not support teleconsultation.");

        var availability = await _doctorsApiClient.GetAvailableSlotsAsync(doctorId, appointmentDate, isTeleConsultation);
        if (availability == null || !availability.Slots.Any(x => x.IsAvailable && x.SlotStartTime == slotStartTime && x.SlotEndTime == slotEndTime))
            throw new InvalidOperationException("Selected slot is not available for the doctor.");

        return doctor;
    }

=======
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
    private static void ValidateCreateRequest(CreateAppointmentRequestDto request)
    {
        if (request == null)
            throw new ArgumentException("Request body is required.");

        if (request.PatientId <= 0)
            throw new ArgumentException("PatientId is required.");

        if (string.IsNullOrWhiteSpace(request.UHID))
            throw new ArgumentException("UHID is required.");

        if (request.DoctorId <= 0)
            throw new ArgumentException("DoctorId is required.");

        if (request.DepartmentId <= 0)
            throw new ArgumentException("DepartmentId is required.");

        if (string.IsNullOrWhiteSpace(request.VisitType))
            throw new ArgumentException("VisitType is required.");

        if (request.AppointmentDate < DateOnly.FromDateTime(DateTime.UtcNow.Date))
            throw new ArgumentException("Appointment date cannot be in the past.");

        if (request.SlotEndTime <= request.SlotStartTime)
            throw new ArgumentException("SlotEndTime must be greater than SlotStartTime.");
    }

    private static void ValidateRescheduleRequest(RescheduleAppointmentRequestDto request)
    {
        if (request.NewAppointmentDate < DateOnly.FromDateTime(DateTime.UtcNow.Date))
            throw new ArgumentException("New appointment date cannot be in the past.");

        if (request.NewSlotEndTime <= request.NewSlotStartTime)
            throw new ArgumentException("NewSlotEndTime must be greater than NewSlotStartTime.");
    }

    private static string? NormalizeNullable(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static AppointmentResponseDto MapToResponse(Appointment appointment)
    {
        return new AppointmentResponseDto
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            UHID = appointment.UHID,
            DoctorId = appointment.DoctorId,
            DoctorName = appointment.DoctorName,
            DepartmentId = appointment.DepartmentId,
            DepartmentName = appointment.DepartmentName,
            AppointmentDate = appointment.AppointmentDate,
            SlotStartTime = appointment.SlotStartTime,
            SlotEndTime = appointment.SlotEndTime,
            VisitType = appointment.VisitType,
            ReasonForVisit = appointment.ReasonForVisit,
            IsTeleConsultation = appointment.IsTeleConsultation,
            Status = appointment.Status,
            CancellationReason = appointment.CancellationReason,
            CompletionNotes = appointment.CompletionNotes,
            CreatedAtUtc = appointment.CreatedAtUtc
        };
    }
<<<<<<< HEAD
}
=======
}
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
