
using Hms.ReceptionApi.DTOs;
using Hms.ReceptionApi.DTOs.Reception;

namespace Hms.ReceptionApi.Interfaces.Clients;

public interface IAppointmentsApiClient
{
    Task<AppointmentSearchResponseDto> SearchAsync(AppointmentSearchRequestDto request);

    Task<BookAppointmentResponseDto> BookAppointmentAsync(AppointmentCreateRequestDto request);
    Task<BookAppointmentResponseDto> RescheduleAppointmentAsync(int appointmentId, RescheduleAppointmentRequestDto request);
    Task<BookAppointmentResponseDto> CancelAppointmentAsync(int appointmentId, CancelAppointmentRequestDto request);
}