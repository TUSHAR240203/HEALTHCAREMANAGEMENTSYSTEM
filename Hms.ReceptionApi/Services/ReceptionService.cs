using FluentValidation;
using Hms.ReceptionApi.DTOs;
using Hms.ReceptionApi.DTOs.Reception;
using Hms.ReceptionApi.Entities;
using Hms.ReceptionApi.Interfaces.Clients;
using Hms.ReceptionApi.Interfaces.Repository;
using Hms.ReceptionApi.Interfaces.Services;
using Hms.ReceptionApi.Validators;
namespace Hms.ReceptionApi.Services;

public class ReceptionService : IReceptionService
{
    private readonly IPatientsApiClient _patientsApiClient;
    private readonly IAppointmentsApiClient _appointmentsApiClient;
    private readonly IAuthApiClient _authApiClient;
    private readonly ICheckInRepository _checkInRepository;
    private readonly IQueueRepository _queueRepository;
    private readonly IBillingApiClient _billingApiClient;

    public ReceptionService(
        IPatientsApiClient patientsApiClient,
        IAppointmentsApiClient appointmentsApiClient,
        IAuthApiClient authApiClient,
        IBillingApiClient billingApiClient,
        ICheckInRepository checkInRepository,
        IQueueRepository queueRepository)
    {
        _patientsApiClient = patientsApiClient;
        _appointmentsApiClient = appointmentsApiClient;
        _authApiClient = authApiClient;
        _billingApiClient = billingApiClient;
        _checkInRepository = checkInRepository;
        _queueRepository = queueRepository;
    }

    public async Task<InvoiceResponseDto> CreateInvoiceAsync(CreateInvoiceRequestDto request)
    {
        if (request.PatientId <= 0)
            throw new ArgumentException("Invalid patient id.");

        if (request.AppointmentId <= 0)
            throw new ArgumentException("Invalid appointment id.");

        if (request.ConsultationFee < 0)
            throw new ArgumentException("Consultation fee cannot be negative.");

        var patient = await _patientsApiClient.GetPatientSummaryAsync(request.PatientId);
        if (patient == null)
            throw new ArgumentException("Patient not found.");

        var invoiceRequest = new
        {
            patientId = patient.PatientId,
            uhid = patient.UHID,
            appointmentId = request.AppointmentId,
            consultationFee = request.ConsultationFee
        };

        return await _billingApiClient.CreateInvoiceAsync(invoiceRequest);
    }

    public async Task<InvoiceResponseDto?> GetInvoiceByIdAsync(int invoiceId)
    {
        if (invoiceId <= 0)
            throw new ArgumentException("Invalid invoice id.");

        return await _billingApiClient.GetInvoiceByIdAsync(invoiceId);
    }

    public async Task<List<InvoiceResponseDto>> GetInvoicesByPatientIdAsync(int patientId)
    {
        if (patientId <= 0)
            throw new ArgumentException("Invalid patient id.");

        return await _billingApiClient.GetInvoicesByPatientIdAsync(patientId);
    }

    public async Task<InvoiceResponseDto> AddInvoiceItemAsync(int invoiceId, AddInvoiceItemRequestDto request)
    {
        if (invoiceId <= 0)
            throw new ArgumentException("Invalid invoice id.");

        if (string.IsNullOrWhiteSpace(request.ServiceName))
            throw new ArgumentException("ServiceName is required.");

        if (request.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.");

        return await _billingApiClient.AddInvoiceItemAsync(invoiceId, request);
    }

    public async Task<InvoiceResponseDto> AddPaymentAsync(int invoiceId, PaymentRequestDto request)
    {
        if (invoiceId <= 0)
            throw new ArgumentException("Invalid invoice id.");

        if (request.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.");

        if (string.IsNullOrWhiteSpace(request.PaymentMode))
            throw new ArgumentException("PaymentMode is required.");

        return await _billingApiClient.AddPaymentAsync(invoiceId, request);
    }

    public async Task<ReceptionPatientSearchResponseDto> SearchPatientsAsync(ReceptionPatientSearchRequestDto request)
    {
        return await _patientsApiClient.SearchPatientsAsync(request);
    }

    public async Task<ReceptionPatientSummaryDto> RegisterPatientAsync(RegisterPatientByReceptionRequestDto request)
    {
        var patient = await _patientsApiClient.RegisterPatientAsync(request);

        if (request.PortalAccessEnabled && request.SendPortalActivationSms)
        {
            await _authApiClient.SendPortalActivationAsync(patient.PatientId);
        }

        return patient;
    }

    public async Task<ReceptionPatientSummaryDto?> GetPatientSummaryAsync(int patientId)
    {
        if (patientId <= 0)
            throw new ArgumentException("Invalid patient id.");

        return await _patientsApiClient.GetPatientSummaryAsync(patientId);
    }

    public async Task<VerifyPatientResponseDto> VerifyPatientAsync(int patientId, VerifyPatientRequestDto request)
    {
        var patient = await _patientsApiClient.GetPatientSummaryAsync(patientId);

        if (patient == null)
        {
            return new VerifyPatientResponseDto
            {
                PatientId = patientId,
                Verified = false,
                Message = "Patient not found."
            };
        }

        var dobMatched = !request.DateOfBirth.HasValue || patient.DateOfBirth == request.DateOfBirth.Value;
        var mobileMatched = string.IsNullOrWhiteSpace(request.MobileNumber) || patient.MobileNumber == request.MobileNumber.Trim();

        var verified = dobMatched && mobileMatched;

        return new VerifyPatientResponseDto
        {
            PatientId = patientId,
            Verified = verified,
            Message = verified ? "Patient identity verified." : "Patient verification failed."
        };
    }

    public async Task ResendPortalActivationAsync(int patientId, ResendPortalActivationRequestDto request)
    {
        var patient = await _patientsApiClient.GetPatientSummaryAsync(patientId);

        if (patient == null)
            throw new ArgumentException("Patient not found.");

        if (!patient.PortalAccessEnabled)
            throw new InvalidOperationException("Portal access is not enabled for this patient.");

        if (patient.PortalActivated)
            throw new InvalidOperationException("Patient portal is already activated.");

        await _authApiClient.SendPortalActivationAsync(patientId);
    }

    public async Task<BookAppointmentResponseDto> BookAppointmentAsync(BookAppointmentRequestDto request)
    {
        var patient = await _patientsApiClient.GetPatientSummaryAsync(request.PatientId);
        if (patient == null)
            throw new ArgumentException("Patient not found.");


        //IValidator<BookAppointmentRequestDto> validator1 = new BookAppointmentRequestDtoValidator();
        /////BookAppointmentRequestDtoValidator validator = new BookAppointmentRequestDtoValidator();
        ///var res = await validator1.ValidateAsync(request);
        //if (!res.IsValid)
        //{
        //   throw new ValidationException(res.Errors);
        //}
        var appointmentRequest = new AppointmentCreateRequestDto
        {
            PatientId = patient.PatientId,   
            UHID = patient.UHID,
            DoctorId = request.DoctorId,
            DoctorName = $"Doctor {request.DoctorId}",
            DepartmentId = request.DepartmentId,
            DepartmentName = $"Department {request.DepartmentId}",
            AppointmentDate = request.AppointmentDate,
            SlotStartTime = request.SlotStartTime,
            SlotEndTime = request.SlotEndTime,
            VisitType = request.VisitType,
            ReasonForVisit = request.ReasonForVisit,
            IsTeleConsultation = request.IsTeleConsultation
        };

        var result = await _appointmentsApiClient.BookAppointmentAsync(appointmentRequest);

        return result;
    }

    public async Task<BookAppointmentResponseDto> RescheduleAppointmentAsync(int appointmentId, RescheduleAppointmentRequestDto request)
    {
        if (appointmentId <= 0)
            throw new ArgumentException("Invalid appointment id.");

        return await _appointmentsApiClient.RescheduleAppointmentAsync(appointmentId, request);
    }

    public async Task<BookAppointmentResponseDto> CancelAppointmentAsync(int appointmentId, CancelAppointmentRequestDto request)
    {
        if (appointmentId <= 0)
            throw new ArgumentException("Invalid appointment id.");

        return await _appointmentsApiClient.CancelAppointmentAsync(appointmentId, request);
    }

    public async Task<CheckInResponseDto> CheckInAsync(CheckInRequestDto request)
    {
        var patient = await _patientsApiClient.GetPatientSummaryAsync(request.PatientId);
        if (patient == null)
            throw new ArgumentException("Patient not found.");

        var queueDate = DateOnly.FromDateTime(request.CheckInTimeUtc);
        var nextToken = await _queueRepository.GetNextTokenNumberAsync(request.DepartmentId, queueDate);

        var checkIn = new PatientCheckIn
        {
            PatientId = request.PatientId,
            UHID = patient.UHID,
            AppointmentId = request.AppointmentId,
            DoctorId = request.DoctorId,
            DepartmentId = request.DepartmentId,
            CheckInTimeUtc = request.CheckInTimeUtc,
            TokenNumber = nextToken,
            Status = "CheckedIn"
        };

        await _checkInRepository.AddAsync(checkIn);
        await _checkInRepository.SaveChangesAsync();

        var queueToken = new QueueToken
        {
            DepartmentId = request.DepartmentId,
            QueueDate = queueDate,
            TokenNumber = nextToken,
            PatientId = request.PatientId,
            UHID = patient.UHID,
            PatientName = patient.FullName,
            AppointmentId = request.AppointmentId,
            DoctorId = request.DoctorId,
            Status = "Waiting"
        };

        await _queueRepository.AddAsync(queueToken);
        await _queueRepository.SaveChangesAsync();

        return new CheckInResponseDto
        {
            CheckInId = checkIn.Id,
            AppointmentId = checkIn.AppointmentId,
            PatientId = checkIn.PatientId,
            UHID = checkIn.UHID,
            TokenNumber = checkIn.TokenNumber,
            QueuePosition = checkIn.TokenNumber,
            Status = checkIn.Status,
            Message = "Patient checked in successfully."
        };
    }

    public async Task<CheckInResponseDto?> GetCheckInByIdAsync(int checkInId)
    {
        var checkIn = await _checkInRepository.GetByIdAsync(checkInId);
        if (checkIn == null)
            return null;

        return new CheckInResponseDto
        {
            CheckInId = checkIn.Id,
            AppointmentId = checkIn.AppointmentId,
            PatientId = checkIn.PatientId,
            UHID = checkIn.UHID,
            TokenNumber = checkIn.TokenNumber,
            QueuePosition = checkIn.TokenNumber,
            Status = checkIn.Status,
            Message = "Check-in record fetched successfully."
        };
    }

    public async Task<DepartmentQueueResponseDto> GetDepartmentQueueAsync(int departmentId, DateOnly date)
    {
        var items = await _queueRepository.GetDepartmentQueueAsync(departmentId, date);

        return new DepartmentQueueResponseDto
        {
            DepartmentId = departmentId,
            DepartmentName = $"Department {departmentId}",
            Date = date,
            Queue = items.Select(x => new QueueItemDto
            {
                TokenNumber = x.TokenNumber,
                PatientId = x.PatientId,
                UHID = x.UHID,
                PatientName = x.PatientName,
                Status = x.Status
            }).ToList()
        };
    }
}