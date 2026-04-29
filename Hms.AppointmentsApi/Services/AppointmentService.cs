using AutoMapper;
using Hms.AppointmentsApi.DTOs.Appointments;
using Hms.AppointmentsApi.Entities;
using Hms.AppointmentsApi.Enums;
using Hms.AppointmentsApi.Interfaces.Repository;
using Hms.AppointmentsApi.Interfaces.Services;
using Hms.AppointmentsApi.Interfaces.Clients;

namespace Hms.AppointmentsApi.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMapper _mapper;
    private readonly IDoctorsApiClient _doctorsApiClient;

    public AppointmentService(
    IAppointmentRepository appointmentRepository,
    IDoctorsApiClient doctorsApiClient,
    IMapper mapper)
    {
        _appointmentRepository = appointmentRepository;
        _doctorsApiClient = doctorsApiClient;
        _mapper = mapper;
    }

    public async Task<AppointmentResponseDto> CreateAsync(CreateAppointmentRequestDto request)
    {
        var availability = await _doctorsApiClient.GetAvailabilityAsync(
            request.DoctorId,
            request.AppointmentDate,
            request.IsTeleConsultation);

        if (availability == null)
            throw new InvalidOperationException("Unable to verify doctor availability.");

        var selectedSlot = availability.Slots.FirstOrDefault(x =>
            x.SlotStartTime == request.SlotStartTime &&
            x.SlotEndTime == request.SlotEndTime);

        if (selectedSlot == null || !selectedSlot.IsAvailable)
        {
            throw new InvalidOperationException(
                availability.Message ?? "Selected appointment slot is not available.");
        }

        var slotBooked = await _appointmentRepository.IsSlotBookedAsync(
            request.DoctorId,
            request.AppointmentDate,
            request.SlotStartTime,
            request.SlotEndTime);

        if (slotBooked)
            throw new InvalidOperationException("This slot is already booked for the doctor.");

        var appointment = _mapper.Map<Appointment>(request);

        await _appointmentRepository.AddAsync(appointment);
        await _appointmentRepository.SaveChangesAsync();

        return _mapper.Map<AppointmentResponseDto>(appointment);
    }

    public async Task<AppointmentResponseDto?> GetByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid appointment id.");

        var appointment = await _appointmentRepository.GetByIdAsync(id);
        return appointment == null ? null : _mapper.Map<AppointmentResponseDto>(appointment);
    }

    public async Task<List<AppointmentResponseDto>> GetByPatientIdAsync(int patientId)
    {
        if (patientId <= 0)
            throw new ArgumentException("Invalid patient id.");

        var appointments = await _appointmentRepository.GetByPatientIdAsync(patientId);
        return _mapper.Map<List<AppointmentResponseDto>>(appointments);
    }

    public async Task<List<AppointmentResponseDto>> GetByDoctorIdAsync(int doctorId)
    {
        if (doctorId <= 0)
            throw new ArgumentException("Invalid doctor id.");

        var appointments = await _appointmentRepository.GetByDoctorIdAsync(doctorId);
        return _mapper.Map<List<AppointmentResponseDto>>(appointments);
    }

    public async Task<AppointmentSearchResponseDto> SearchAsync(AppointmentSearchRequestDto request)
    {
        request.PageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        request.PageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        request.PageSize = request.PageSize > 100 ? 100 : request.PageSize;

        return await _appointmentRepository.SearchAsync(request);
    }

    public async Task<AppointmentResponseDto?> RescheduleAsync(int id, RescheduleAppointmentRequestDto request)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid appointment id.");

        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null)
            return null;

        if (appointment.Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Cancelled appointment cannot be rescheduled.");

        if (appointment.Status == AppointmentStatus.Completed)
            throw new InvalidOperationException("Completed appointment cannot be rescheduled.");

        var availability = await _doctorsApiClient.GetAvailabilityAsync(
    appointment.DoctorId,
    request.NewAppointmentDate,
    appointment.IsTeleConsultation);

        if (availability == null)
            throw new InvalidOperationException("Unable to verify doctor availability.");

        var selectedSlot = availability.Slots.FirstOrDefault(x =>
            x.SlotStartTime == request.NewSlotStartTime &&
            x.SlotEndTime == request.NewSlotEndTime);

        if (selectedSlot == null || !selectedSlot.IsAvailable)
        {
            throw new InvalidOperationException(
                availability.Message ?? "Selected appointment slot is not available.");
        }

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

        return _mapper.Map<AppointmentResponseDto>(appointment);
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

        return _mapper.Map<AppointmentResponseDto>(appointment);
    }

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

        return _mapper.Map<AppointmentResponseDto>(appointment);
    }

    private static string? NormalizeNullable(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
