using Hms.ReceptionApi.DTOs;
<<<<<<< HEAD
using Hms.ReceptionApi.DTOs.Doctors;
=======
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
using Hms.ReceptionApi.DTOs.Reception;
using Hms.ReceptionApi.Entities;
using Hms.ReceptionApi.Interfaces.Clients;
using Hms.ReceptionApi.Interfaces.Repository;
using Hms.ReceptionApi.Interfaces.Services;

namespace Hms.ReceptionApi.Services;

public class ReceptionService : IReceptionService
{
    private readonly IPatientsApiClient _patientsApiClient;
    private readonly IAppointmentsApiClient _appointmentsApiClient;
    private readonly IAuthApiClient _authApiClient;
<<<<<<< HEAD
    private readonly IDoctorsApiClient _doctorsApiClient;
=======
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
    private readonly ICheckInRepository _checkInRepository;
    private readonly IQueueRepository _queueRepository;
    private readonly IBillingApiClient _billingApiClient;

    public ReceptionService(
        IPatientsApiClient patientsApiClient,
        IAppointmentsApiClient appointmentsApiClient,
        IAuthApiClient authApiClient,
<<<<<<< HEAD
        IDoctorsApiClient doctorsApiClient,
=======
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
        IBillingApiClient billingApiClient,
        ICheckInRepository checkInRepository,
        IQueueRepository queueRepository)
    {
        _patientsApiClient = patientsApiClient;
        _appointmentsApiClient = appointmentsApiClient;
        _authApiClient = authApiClient;
<<<<<<< HEAD
        _doctorsApiClient = doctorsApiClient;
=======
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
        _billingApiClient = billingApiClient;
        _checkInRepository = checkInRepository;
        _queueRepository = queueRepository;
    }

<<<<<<< HEAD
    public async Task<List<DoctorSummaryDto>> SearchDoctorsAsync(DoctorSearchRequestDto request)
    {
        request ??= new DoctorSearchRequestDto { IsActive = true };
        request.IsActive ??= true;
        return await _doctorsApiClient.SearchDoctorsAsync(request);
    }

    public async Task<DoctorSummaryDto?> GetDoctorByIdAsync(int doctorId)
    {
        if (doctorId <= 0)
            throw new ArgumentException("Invalid doctor id.");

        return await _doctorsApiClient.GetDoctorByIdAsync(doctorId);
    }

    public async Task<DoctorAvailabilityResponseDto?> GetDoctorAvailableSlotsAsync(int doctorId, DateOnly date, bool isTeleConsultation)
    {
        if (doctorId <= 0)
            throw new ArgumentException("Invalid doctor id.");

        return await _doctorsApiClient.GetAvailableSlotsAsync(doctorId, date, isTeleConsultation);
    }

=======
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
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

<<<<<<< HEAD
        var doctor = await _doctorsApiClient.GetDoctorByIdAsync(request.DoctorId);
        if (doctor == null)
            throw new ArgumentException("Doctor not found.");

        if (!doctor.IsActive)
            throw new InvalidOperationException("Doctor is inactive.");

        if (doctor.DepartmentId != request.DepartmentId)
            throw new InvalidOperationException("Selected doctor does not belong to the provided department.");

        var availability = await _doctorsApiClient.GetAvailableSlotsAsync(request.DoctorId, request.AppointmentDate, request.IsTeleConsultation);
        if (availability == null || !availability.Slots.Any(x => x.IsAvailable && x.SlotStartTime == request.SlotStartTime && x.SlotEndTime == request.SlotEndTime))
            throw new InvalidOperationException("Selected slot is not available for the doctor.");

=======
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
        var appointmentRequest = new AppointmentCreateRequestDto
        {
            PatientId = patient.PatientId,
            UHID = patient.UHID,
<<<<<<< HEAD
            DoctorId = doctor.Id,
            DoctorName = doctor.FullName,
            DepartmentId = doctor.DepartmentId,
            DepartmentName = doctor.DepartmentName,
=======
            DoctorId = request.DoctorId,
            DoctorName = $"Doctor {request.DoctorId}",
            DepartmentId = request.DepartmentId,
            DepartmentName = $"Department {request.DepartmentId}",
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
            AppointmentDate = request.AppointmentDate,
            SlotStartTime = request.SlotStartTime,
            SlotEndTime = request.SlotEndTime,
            VisitType = request.VisitType,
            ReasonForVisit = request.ReasonForVisit,
            IsTeleConsultation = request.IsTeleConsultation
        };

<<<<<<< HEAD
        return await _appointmentsApiClient.BookAppointmentAsync(appointmentRequest);
=======
        var result = await _appointmentsApiClient.BookAppointmentAsync(appointmentRequest);

        return result;
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
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
<<<<<<< HEAD
        if (checkIn == null) return null;
=======
        if (checkIn == null)
            return null;
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85

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
<<<<<<< HEAD
}
=======
}
>>>>>>> ee49ab9fb4705d2037d437f343847efd9ce49e85
