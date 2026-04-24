using Hms.ReceptionApi.DTOs.Doctors;
using Hms.ReceptionApi.DTOs.Reception;

namespace Hms.ReceptionApi.Interfaces.Services;

public interface IReceptionService
{
    Task<ReceptionPatientSearchResponseDto> SearchPatientsAsync(ReceptionPatientSearchRequestDto request);
    Task<ReceptionPatientSummaryDto> RegisterPatientAsync(RegisterPatientByReceptionRequestDto request);
    Task<ReceptionPatientSummaryDto?> GetPatientSummaryAsync(int patientId);
    Task<VerifyPatientResponseDto> VerifyPatientAsync(int patientId, VerifyPatientRequestDto request);
    Task ResendPortalActivationAsync(int patientId, ResendPortalActivationRequestDto request);

    Task<List<DoctorSummaryDto>> SearchDoctorsAsync(DoctorSearchRequestDto request);
    Task<DoctorSummaryDto?> GetDoctorByIdAsync(int doctorId);
    Task<DoctorAvailabilityResponseDto?> GetDoctorAvailableSlotsAsync(int doctorId, DateOnly date, bool isTeleConsultation);

    Task<BookAppointmentResponseDto> BookAppointmentAsync(BookAppointmentRequestDto request);
    Task<BookAppointmentResponseDto> RescheduleAppointmentAsync(int appointmentId, RescheduleAppointmentRequestDto request);
    Task<BookAppointmentResponseDto> CancelAppointmentAsync(int appointmentId, CancelAppointmentRequestDto request);

    Task<CheckInResponseDto> CheckInAsync(CheckInRequestDto request);
    Task<CheckInResponseDto?> GetCheckInByIdAsync(int checkInId);
    Task<DepartmentQueueResponseDto> GetDepartmentQueueAsync(int departmentId, DateOnly date);

    Task<InvoiceResponseDto> CreateInvoiceAsync(CreateInvoiceRequestDto request);
    Task<InvoiceResponseDto?> GetInvoiceByIdAsync(int invoiceId);
    Task<List<InvoiceResponseDto>> GetInvoicesByPatientIdAsync(int patientId);
    Task<InvoiceResponseDto> AddInvoiceItemAsync(int invoiceId, AddInvoiceItemRequestDto request);
    Task<InvoiceResponseDto> AddPaymentAsync(int invoiceId, PaymentRequestDto request);
}